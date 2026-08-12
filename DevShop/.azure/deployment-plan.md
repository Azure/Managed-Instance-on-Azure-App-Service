# Azure Deployment Plan

> **Status:** Ready for Validation

Generated: 2026-08-10

## Current revision

Remove the obsolete custom `DevShopMcpProxy` project and tracked `.mcp\mcp.json` configuration because App Service built-in MCP is now the only MCP hosting path. Replace the ignored generated OpenAPI files with a committed, read-only OpenAPI 3.0.3 document named `openapi_mcp.json`, include it in the Visual Studio project, and simplify the README to upload that file directly.

Files removed: `DevShopMcpProxy/**`, `.mcp/mcp.json`, local proxy build outputs, and ignored generated Swagger/OpenAPI files. Files created or updated: `openapi_mcp.json`, `devShop.csproj`, `devShop.wpp.targets`, `README.md`, and this plan. No Azure resources were changed or deployed.

## 1. Project overview

**Goal:** Publish a verified, public-facing deployment guide for the existing devShop sample. The guide will provision the repository's Azure resources, initialize Azure SQL, create an App Service Managed Instance plan and Windows web app, configure protected settings, build the .NET Framework 4.8 application, and deploy a ready-to-run ZIP package.

**Path:** Modify an existing Azure-enabled application. This task prepares documentation and supporting repository fixes only; it does not deploy Azure resources.

## 2. Requirements

| Attribute | Value |
|---|---|
| Classification | Public sample / proof of concept |
| Scale | Small, single-region |
| Budget | Template-defined development footprint; warn readers that Bastion, Azure SQL GP Gen5, and P1V4 incur ongoing charges |
| Subscription | Selected by each reader at deployment time |
| Location | Reader selects a currently supported App Service Managed Instance preview region |
| Compliance | No production compliance posture is claimed; readers must apply their own policy, residency, and governance requirements |

## 3. Components detected

| Component | Type | Technology | Path |
|---|---|---|---|
| devShop | Web UI, REST API, and built-in MCP source | ASP.NET Web Forms/Web API, .NET Framework 4.8, OpenAPI 3.0.3 | `devShop.csproj`, `openapi_mcp.json` |
| Base infrastructure | ARM template | VNet, Bastion, Azure SQL, Storage, Key Vault, user-assigned identity | `ARM/MIonAppSVC.json` |
| Managed plan | ARM template | App Service Managed Instance preview, Windows P1V4 | `ARM/MIonAppSVCasmiarm.json` |
| Database | T-SQL | Schema and sample data | `SQL` |

## 4. Recipe selection

**Selected:** Existing ARM templates plus Azure CLI and PowerShell commands.

**Rationale:** The repository already contains user-authored ARM templates for preview-only App Service Managed Instance properties. Converting them to AZD/Bicep would be unrelated to the requested README and could alter preview behavior.

## 5. Architecture

**Stack:** Windows App Service Managed Instance hosting a .NET Framework 4.8 web app.

| Component | Azure service | SKU/configuration |
|---|---|---|
| Web application | App Service Managed Instance web app | Windows, ASP.NET 4.8 |
| Compute plan | `Microsoft.Web/serverfarms` | P1V4, one instance, custom mode |
| Database | Azure SQL Database | General Purpose Gen5, 4 vCores |
| Secrets | Azure Key Vault | Standard, RBAC enabled |
| Identity | User-assigned managed identity | Plan startup and Key Vault access |
| Files/scripts | Storage account, Blob, Azure Files | Standard LRS |
| Network | VNet and delegated application subnet | `Microsoft.Web/serverFarms`; service endpoints for SQL, Storage, and Key Vault |
| Administration | Azure Bastion | Standard, two scale units |

The base template defaults to Entra-only SQL with a user-assigned managed identity for application database access and Key Vault secret resolution. SQL username/password authentication remains an explicit optional compatibility fallback, with credentials supplied only through an ignored local parameter file.

## 6. Provisioning-limit checklist

This documentation task deploys zero resources, so no subscription quota is consumed and no quota query is applicable. The README will require readers to confirm preview-region availability, provider registration, policy compatibility, and P1V4 capacity in their own subscription before deployment.

| Resource type changed by this task | Number deployed | Total after task | Limit/quota | Status |
|---|---:|---:|---|---|
| Azure resources | 0 | Unchanged | Not applicable | Within limits |

## 7. Planned repository changes

1. Expand `README.md` with prerequisites, permissions, preview limitations, secure variables, provider registration, parameter preparation, base ARM deployment, output discovery, bootstrap script upload, SQL initialization, managed-plan deployment, web-app creation, identity/Key Vault configuration, build, ZIP deployment, verification, troubleshooting, cleanup, and official references.
2. Correct the public parameter example so its prefix satisfies the 24-character Storage account naming limit.
3. Add the missing `ProductReviews` schema to `SQL/CreateTables.sql`, because the deployed API queries this table and a fresh deployment currently cannot use review endpoints.
4. Explain the SQL script order and explicitly exclude `SQL/results_inserts.sql`, which targets a legacy `dbo.Inventory` table that the current schema does not create.
5. Correct the legacy web publish target and prevent infrastructure, SQL scripts, repository documentation, MCP configuration, and C# source from being copied into the deployed web root.
6. Correct ARM extension-resource syntax and scope Key Vault and Blob data-plane roles to the individual resources.
7. Document secure App Service built-in MCP configuration, Visual Studio connection and testing, read-only sample prompts, and agentic workflow usage.
8. Remove the custom MCP proxy and obsolete MCP client configuration, commit one read-only built-in MCP OpenAPI document, and update all references.

## 8. Validation

- Parse both ARM templates and the parameter example as JSON.
- Run the documented MSBuild publish command and inspect the ZIP layout.
- Validate the SQL schema/script ordering statically and ensure all application-referenced tables are created.
- Check every documented repository path and PowerShell variable.
- Build the solution after the schema/documentation changes.
- Do not run Azure deployment commands or create cloud resources.

## 9. Execution checklist

- [x] Analyze workspace and existing infrastructure
- [x] Scan application configuration and SQL access
- [x] Verify App Service Managed Instance constraints against official Microsoft documentation
- [x] Select ARM plus Azure CLI recipe
- [x] Plan architecture and documentation scope
- [x] User approves this plan
- [x] Update README and supporting deployment files
- [x] Rename all `build2026*` files and update repository references
- [x] Make managed identity the default database authentication mode
- [x] Validate documentation, templates, schema, build, and publish package
- [x] Enforce HTTPS in the public web-app creation instructions
- [x] Add and validate built-in MCP and Visual Studio instructions
- [x] Receive explicit approval for proxy and generated-output deletion
- [x] Remove proxy files and obsolete references
- [x] Add and validate tracked `openapi_mcp.json`

## 10. Files

| File | Purpose | Planned status |
|---|---|---|
| `.azure/deployment-plan.md` | Source-of-truth preparation plan | Created |
| `README.md` | Public deployment guide | Update |
| `ARM/MIonAppSVC.json` | Managed-identity-first SQL configuration and least-privilege role assignments | Rename and update |
| `ARM/MIonAppSVCARM.parameters.example.json` | Passwordless public parameter example | Rename and update |
| `ARM/MIonAppSVCasmiarm.json` | Managed plan template | Rename |
| `SQL/CreateTables.sql` | Complete fresh-deployment schema | Update |
| `openapi_mcp.json` | Read-only OpenAPI 3.0.3 source for App Service built-in MCP | Create |
| `devShop.wpp.targets` | Exclude repository-only files from web publish | Create |
| `DevShopMcpProxy/**` | Obsolete custom MCP server | Remove |
| `.mcp/mcp.json` | Obsolete placeholder client configuration | Remove |

## 11. Azure validation steps

- [x] Azure CLI installation (`az` 2.77.0)
- [x] Azure CLI authentication is active
- [x] ARM templates and parameter example parse as JSON
- [x] Application restores and builds in Release configuration
- [x] Web Publishing Pipeline creates a ready-to-run ZIP with repository-only files excluded
- [x] Static RBAC review confirms Key Vault Secrets User and Storage Blob Data Reader at resource scope
- [ ] Resource-group template validation requires reader-selected subscription, resource group, preview region, and secure parameters
- [ ] What-if and Azure Policy validation require the same reader-selected Azure context
- [ ] Preview availability and P1V4 regional capacity must be checked at deployment time

Cloud validation was not run because this task changes documentation and repository deployment assets only and explicitly does not deploy or target a specific subscription. The plan remains `Ready for Validation`; it must not be treated as authorization to deploy.

## 12. Validation proof

| Check | Command | Result | Date |
|---|---|---|---|
| ARM/parameter syntax | PowerShell `ConvertFrom-Json` for both templates and the example | Pass | 2026-08-10 |
| Project metadata | PowerShell XML parse of `devShop.csproj` and `devShop.wpp.targets` | Pass | 2026-08-10 |
| Restore/build | Visual Studio MSBuild `Restore` and Release build | Pass | 2026-08-10 |
| Publish package | MSBuild `WebPublish`, `Compress-Archive`, and ZIP entry inspection | Pass; 121 runtime entries and no ARM, SQL, MCP, documentation, or C# source | 2026-08-10 |
| Schema coverage | Static cross-check of application table references and `SQL/CreateTables.sql` | Pass | 2026-08-10 |
| RBAC | Static inspection of role IDs, principal type, top-level extension scope, and dependencies | Pass | 2026-08-10 |
| Built-in MCP specification | Parse `openapi_mcp.json`; compare methods, operation allowlist, metadata, and size | Pass; 8 GET tools, no write routes or metadata, 10,714 bytes, OpenAPI 3.0.3 | 2026-08-10 |
| MCP cleanup review | Independent correctness review of proxy removal, project references, publish exclusion, and README guidance | Initial project-reference issue corrected; no remaining material findings | 2026-08-10 |
| Diff integrity | `git diff --check` and CRLF verification | Pass | 2026-08-10 |
| Azure group validate/what-if/policy | Not run without user-selected deployment context | Pending at deployment time | 2026-08-10 |

The local Bicep decompiler was also probed but is not used as proof: decompiling this hand-authored ARM template cannot losslessly represent its inline and child subnet resources and reports generated Bicep dependency cycles. The source ARM JSON remains the deployment artifact.
