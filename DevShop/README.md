# devShop

devShop is a sample ASP.NET Web Forms and Web API application targeting .NET Framework 4.8. This guide deploys its Azure dependencies, creates an **App Service Managed Instance** plan and web app, initializes the database, and publishes the application from Visual Studio.

**Managed identity authentication is the recommended and default database configuration.** It avoids storing a database password and supports Entra-only Azure SQL policies. SQL username/password authentication remains available only as an optional compatibility fallback.

> [!IMPORTANT]
> Managed Instance on Azure App Service is generally available for Windows web apps in select regions on Pv4 and Pmv4 pricing plans. Managed Instance doesn't support Linux or containers. Deploy every resource for this sample to the same supported region, and confirm current availability in the <a href="https://learn.microsoft.com/azure/app-service/quickstart-managed-instance">Managed Instance quickstart</a>.

## What is deployed

- Virtual network and delegated App Service subnet
- Azure SQL logical server and database
- Storage account, Blob container, and Azure Files shares
- Key Vault and user-assigned managed identity
- Azure Bastion
- App Service Managed Instance P1V4 plan
- Windows web app running ASP.NET 4.8

These resources incur Azure charges. This is a sample deployment, not a production reference architecture.

## Prerequisites

- Azure subscription with permission to create resources and role assignments
- Azure CLI
- Visual Studio 2022 or later with the **ASP.NET and web development** workload and .NET Framework 4.8 targeting pack
- Git
- A SQL client, or access to the Azure portal SQL Query editor

## 1. Clone the repository

```powershell
git clone https://github.com/gsethdev/devShop.git
Set-Location .\devShop
```

## 2. Prepare the deployment parameters

Sign in, then display the Entra values used to administer the new database:

```powershell
az login
az ad signed-in-user show --query '{login:displayName, objectId:id}' --output json
```

Copy the passwordless public example to the ignored local parameter file:

```powershell
Copy-Item .\ARM\MIonAppSVCARM.parameters.example.json .\ARM\MIonAppSVCARM.parameters.json
notepad .\ARM\MIonAppSVCARM.parameters.json
```

Update these values:

| Parameter | Value |
|---|---|
| `location` | A region where App Service Managed Instance is available |
| `resourcePrefix` | Up to six lowercase letters or numbers, for example `devshp` |
| `databaseAuthenticationMode` | Keep `ManagedIdentity` (recommended) |
| `sqlEntraAdministratorLogin` | The `login` value returned above |
| `sqlEntraAdministratorObjectId` | The `objectId` value returned above |
| `sqlEntraAdministratorPrincipalType` | `User`; use `Group` or `Application` only when appropriate |
| `sqlDatabaseName` | Keep `devShop`, unless you intentionally change it |

The SQL username and password are optional and are not present in the managed-identity example. To use the less-secure fallback, set `databaseAuthenticationMode` to `SqlPassword` and add `sqladminUsername` and `sqlAdministratorLoginPassword` values to the local file.

The local parameter file is ignored by Git. Never commit it or any SQL password.

## 3. Deploy the Azure dependencies

Sign in, select the subscription, and create a resource group in the same region used in the parameter file:

```powershell
az account set --subscription '<subscription-id>'

$ResourceGroup = 'rg-devshop-asmi'
$Location = 'northeurope'   # Must match the parameter file

az group create --name $ResourceGroup --location $Location
```

Deploy the dependency template:

```powershell
az deployment group create `
  --resource-group $ResourceGroup `
  --template-file .\ARM\MIonAppSVC.json `
  --parameters '@ARM/MIonAppSVCARM.parameters.json'
```

When deployment completes, open the resource group in the Azure portal and confirm that SQL, Storage, Key Vault, the virtual network, managed identity, and Bastion resources exist.

## 4. Create and populate the database

1. In the Azure portal, open the generated **SQL server**.
2. Under **Networking**, temporarily add your current client IPv4 address.
3. Open the `devShop` database and select **Query editor**, or connect with SQL Server Management Studio/Azure Data Studio.
4. For `ManagedIdentity` mode, sign in with the Entra administrator configured in the parameter file. For the optional `SqlPassword` mode, use the SQL administrator credentials.
5. Run these scripts in order:
   1. `SQL\CreateTables.sql`
   2. `SQL\PopulateTables.sql`
   3. `SQL\CreateProductInventory.sql`
6. When using `ManagedIdentity`, discover the application identity:

   ```powershell
   $IdentityId = az resource list -g $ResourceGroup --resource-type Microsoft.ManagedIdentity/userAssignedIdentities --query '[0].id' -o tsv
   $IdentityName = az identity show --ids $IdentityId --query name -o tsv
   $IdentityObjectId = az identity show --ids $IdentityId --query principalId -o tsv
   ```

7. Still connected as the Entra administrator, grant that identity only the data access required by the app. Replace the placeholders with the values above:

   ```sql
   CREATE USER [<identity-name>] FROM EXTERNAL PROVIDER;
   ALTER ROLE db_datareader ADD MEMBER [<identity-name>];
   ALTER ROLE db_datawriter ADD MEMBER [<identity-name>];
   ```

   If your tenant contains duplicate identity display names, use `CREATE USER [<identity-name>-app] FROM EXTERNAL PROVIDER WITH OBJECT_ID = '<identity-object-id>';` and grant the roles to that alias.
8. Verify that `Categories`, `Products`, `Orders`, `ProductInventory`, and `ProductReviews` exist.
9. Remove the temporary SQL firewall rule.

> [!NOTE]
> Do not run `SQL\results_inserts.sql`. It targets a legacy table named `dbo.Inventory`; this application uses `dbo.ProductInventory`.

## 5. Upload the installation package

App Service Managed Instance expects an installation ZIP in the private `scriptcontainer` Blob container. This application needs no additional machine software, so create a minimal package:

```powershell
New-Item -ItemType Directory -Force .\artifacts\bootstrap | Out-Null
"Write-Output 'devShop bootstrap completed.'" | Set-Content .\artifacts\bootstrap\Install.ps1
Compress-Archive .\artifacts\bootstrap\Install.ps1 .\artifacts\installcomponents.zip -Force
```

Upload it from the Azure portal:

1. Open the generated storage account.
2. Under **Networking**, temporarily allow your client IP.
3. Open **Storage browser** > **Blob containers** > `scriptcontainer`.
4. Upload `artifacts\installcomponents.zip`.
5. Copy the blob URL; it is needed in the next step.
6. Remove the temporary storage network rule.

Keep the container private. The managed plan uses its assigned identity to read the package.

## 6. Create the App Service Managed Instance plan

Set the plan name and discover the generated dependency values:

```powershell
$PlanName = 'devshop-asmi-plan'
$StorageName = az resource list -g $ResourceGroup --resource-type Microsoft.Storage/storageAccounts --query '[0].name' -o tsv
$VaultName = az resource list -g $ResourceGroup --resource-type Microsoft.KeyVault/vaults --query '[0].name' -o tsv
$IdentityId = az resource list -g $ResourceGroup --resource-type Microsoft.ManagedIdentity/userAssignedIdentities --query '[0].id' -o tsv
$IdentityClientId = az identity show --ids $IdentityId --query clientId -o tsv
$VnetName = az resource list -g $ResourceGroup --resource-type Microsoft.Network/virtualNetworks --query '[0].name' -o tsv
$SubnetId = az network vnet subnet show -g $ResourceGroup --vnet-name $VnetName -n default2 --query id -o tsv
$InstallScriptUri = '<blob-url-copied-in-step-5>'
```

Deploy the managed plan:

```powershell
az deployment group create `
  --resource-group $ResourceGroup `
  --template-file .\ARM\MIonAppSVCasmiarm.json `
  --parameters `
    location=$Location `
    planName=$PlanName `
    virtualNetworkSubnetId=$SubnetId `
    databaseSecretUri="https://$VaultName.vault.azure.net/secrets/SqlConnectionString" `
    installScriptUri=$InstallScriptUri `
    storageAccountName=$StorageName `
    fileShareSecretUri="https://$VaultName.vault.azure.net/secrets/FileShareConnectionString" `
    userAssignedIdentityResourceId=$IdentityId
```

The template creates a Windows P1V4 plan, keeps RDP disabled, configures the registry adapter, and mounts the Azure Files and local drives required by the application.

## 7. Create and configure the web app

Create a globally unique web app name:

```powershell
$WebAppName = 'devshop-<unique-suffix>'

az webapp create `
  --resource-group $ResourceGroup `
  --plan $PlanName `
  --name $WebAppName `
  --runtime 'ASPNET:V4.8'

az webapp update `
  --resource-group $ResourceGroup `
  --name $WebAppName `
  --https-only true

az webapp identity assign `
  --resource-group $ResourceGroup `
  --name $WebAppName `
  --identities $IdentityId

$WebAppId = az webapp show -g $ResourceGroup -n $WebAppName --query id -o tsv
az resource update --ids $WebAppId --set properties.keyVaultReferenceIdentity=$IdentityId
```

The base template already grants this user-assigned identity **Key Vault Secrets User**. In the Azure portal:

1. Open the Key Vault and select **Access control (IAM)**.
2. Assign **Key Vault Secrets Officer** to your deployment account so you can create the API-key secret.
3. Under **Networking**, temporarily allow your current client IPv4 address.
4. Create a secret named `DevShopApiKey` with a strong random value.
5. Remove the temporary Key Vault network rule.
6. Open the web app's **Environment variables** page and add:

| Type | Name | Value |
|---|---|---|
| Connection string (`SQLAzure`) | `DBConnection` | `@Microsoft.KeyVault(SecretUri=https://<vault-name>.vault.azure.net/secrets/SqlConnectionString)` |
| App setting | `ApiKey` | `@Microsoft.KeyVault(SecretUri=https://<vault-name>.vault.azure.net/secrets/DevShopApiKey)` |

Replace `<vault-name>` with the generated Key Vault name, save the changes, and restart the web app.

## 8. Publish from Visual Studio

1. Open `devShop.sln` in Visual Studio.
2. Build the solution in **Release** configuration.
3. In Solution Explorer, right-click **devShop** and select **Publish**.
4. Create a new profile and select **Azure**.
5. Select **Azure App Service (Windows)**.
6. Select the web app created in step 7.
7. Select **Finish**, then **Publish**.

The project uses `devShop.wpp.targets` to keep ARM templates, SQL scripts, MCP configuration, documentation, and C# source out of the deployed web root.

## 9. Verify the deployment

Open:

```text
https://<web-app-name>.azurewebsites.net
```

Confirm that:

- The home page displays products.
- Selecting a product opens its details page.
- Completing a purchase displays the confirmation page.
- `/api/inventory` returns inventory data.
- `/api/reviews` returns review data or an empty list.
- `/swagger` opens the API documentation.

Review creation and inventory updates require the `X-Api-Key` header containing the `DevShopApiKey` secret value.

## 10. Configure App Service built-in MCP

[App Service built-in MCP](https://learn.microsoft.com/azure/app-service/configure-mcp-built-in?tabs=portal) converts this app's REST operations into MCP tools without deploying a separate MCP server. This feature is in preview and supports OpenAPI 3.0.x over streamable HTTP.

> [!IMPORTANT]
> Protect the MCP endpoint with [App Service Authentication and MCP authorization](https://learn.microsoft.com/azure/app-service/configure-authentication-mcp) before making it available outside an isolated lab. Built-in MCP does not make the underlying API routes secure. Keep write operations disabled unless you deliberately add compatible authorization and approval controls.

### OpenAPI specification

The tracked `openapi_mcp.json` file is an OpenAPI 3.0.3 document containing eight read-only inventory and review operations. It intentionally excludes inventory updates and review creation, so the built-in MCP tool surface is read-only by construction. The file is visible in the `devShop` project in Solution Explorer and is small enough for portal upload.

### Enable the built-in MCP server

1. In the Azure portal, open the deployed web app.
2. Under **Settings**, select **AI (preview)**, then **MCP servers**.
3. Select **+ Create MCP server**.
4. Set **Display name** to `devshop`, **Endpoint path** to `/mcp/devshop`, and keep **API spec path** as `/home/data/.ai/apispec.json`.
5. Upload `openapi_mcp.json` from the repository root, then select **Create MCP**.
6. Expand the new server and confirm that exactly these eight read-only tools were discovered and enabled:

   `GetAllInventory`, `GetProductInventory`, `GetInventoryByCategory`, `CheckStockAvailability`, `GetLowStockProducts`, `GetProductReviews`, `GetAllReviews`, and `GetReviewsByCategory`.

The MCP endpoint is:

```text
https://<web-app-name>.azurewebsites.net/mcp/devshop
```

If the eight tools aren't discovered, verify that the uploaded document is valid OpenAPI 3.0.3 and wasn't truncated. After changing the specification, App Service automatically refreshes the tool list.

### Connect and test from Visual Studio

Visual Studio 2026 and Visual Studio 2022 17.14 or later can use remote MCP servers in GitHub Copilot agent mode. Either add the endpoint through **Copilot Chat** > **Agent** > **Tools** > **+** > **Add custom MCP server**, or create a local `<SOLUTIONDIR>\.mcp.json` file:

```json
{
  "servers": {
    "devshop-built-in": {
      "url": "https://<web-app-name>.azurewebsites.net/mcp/devshop"
    }
  }
}
```

Use the solution-root `.mcp.json` location shown above and keep deployment-specific endpoints and credentials out of source control.

1. Save `.mcp.json` and complete the **Authentication Required** CodeLens flow if prompted.
2. Open Copilot Chat and select **Agent** mode.
3. Select **Tools**, expand `devshop-built-in`, and enable the read-only tools. MCP tools are disabled by default.
4. Try these prompts and approve tool calls only after reviewing them:

   - `Use the devShop tools to list products with stock below 5. Group them by category and recommend a restocking order. Do not modify inventory.`
   - `Check product 1 inventory for sizes M and L, then summarize which size needs attention.`
   - `Compare product 1 inventory with its customer reviews and identify any demand or quality signals.`
   - `Create a Markdown operations report from low-stock inventory and recent reviews. Save it locally, but do not call any write API.`

MCP tools can participate in larger agentic workflows alongside Visual Studio's file, code, build, test, and other enabled MCP tools. For example, an agent can retrieve live inventory, correlate it with reviews, generate a restocking report, and update a local planning file in one workflow. Keep the tool allowlist minimal, treat API and review content as untrusted input, inspect the agent's proposed actions, and require confirmation for changes with business impact.

## Troubleshooting

| Problem | Check |
|---|---|
| Managed plan deployment fails | Managed Instance is available in the selected region, P1V4 capacity is available, and all resources use the same region |
| Installation ZIP cannot be read | The blob is private, the URL is correct, and the managed identity has Storage Blob Data Reader |
| Database connection fails | `DBConnection` is a resolved Key Vault reference and the SQL scripts completed successfully |
| Key Vault reference is unresolved | The web app identity has Key Vault Secrets User and can reach the vault through the managed plan subnet |
| Publish succeeds but the app fails | The ZIP/site root contains `Web.config`, `Default.aspx`, and `bin\devShop.dll` |

## Cleanup

Deleting the resource group permanently deletes the application, database, and all supporting resources:

```powershell
az group delete --name $ResourceGroup --yes --no-wait
Remove-Item .\ARM\MIonAppSVCARM.parameters.json -Force -ErrorAction SilentlyContinue
Remove-Item .\artifacts -Recurse -Force -ErrorAction SilentlyContinue
```

## Security and license

Report vulnerabilities privately as described in [SECURITY.md](SECURITY.md). Never commit credentials, real parameter files, publish profiles, or production resource identifiers.

Licensed under the [MIT License](LICENSE).