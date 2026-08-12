# Azure Server URL Configuration

## Overview

The SwaggerConfig now automatically detects whether the application is running on Azure and configures the correct server URL in the OpenAPI specification.

## How It Works

### Environment Detection

The configuration checks for the `WEBSITE_HOSTNAME` environment variable:

```csharp
var websiteHostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
```

This environment variable is **automatically set by Azure App Service** and contains your app's hostname.

### Server URL Configuration

#### When Running on Azure:
```csharp
if (!string.IsNullOrEmpty(websiteHostname))
{
    // Returns: https://your-app-name.azurewebsites.net
    return $"https://{websiteHostname}";
}
```

#### When Running Locally:
```csharp
else
{
  // Returns: http://localhost:6294 (or your local port)
    return $"{scheme}://{host}{portPart}";
}
```

## OpenAPI Output

### Local Development:
```json
{
  "swagger": "2.0",
  "info": { "version": "v1", "title": "devShop Product Reviews API" },
  "host": "localhost:6294",
  "schemes": ["http"],
  "basePath": "/",
  ...
}
```

### Azure Production:
```json
{
  "swagger": "2.0",
  "info": { "version": "v1", "title": "devShop Product Reviews API" },
  "host": "your-app-name.azurewebsites.net",
  "schemes": ["https"],
  "basePath": "/",
  "x-azure-deployment": {
    "hostname": "your-app-name.azurewebsites.net",
    "platform": "Azure App Service"
  },
  ...
}
```

## Azure AI Foundry Compatibility

### Why This Matters:

Azure AI Foundry Agent Service needs to know where to send API requests. With the `WEBSITE_HOSTNAME` environment variable:

1. ? **Correct Server URL** - Always points to the actual deployment
2. ? **HTTPS Enforcement** - Uses HTTPS on Azure automatically
3. ? **No Manual Configuration** - Works automatically when deployed
4. ? **AI Agent Discovery** - Agent knows where to call functions

### Agent Function Invocation:

When Azure AI Foundry imports your OpenAPI spec:

```json
{
  "operationId": "GetProductReviews",
  "x-ms-visibility": "important",
  "x-ms-endpoint": "https://your-app-name.azurewebsites.net/api/products/{productId}/reviews"
}
```

The agent will automatically call:
```
https://your-app-name.azurewebsites.net/api/products/123/reviews
```

## Testing

### Local Testing:
```bash
# Server URL will be: http://localhost:6294
curl http://localhost:6294/swagger/docs/v1 | jq '.host'
# Output: "localhost:6294"
```

### Azure Testing:
```bash
# Server URL will be: https://your-app-name.azurewebsites.net
curl https://your-app-name.azurewebsites.net/swagger/docs/v1 | jq '.host'
# Output: "your-app-name.azurewebsites.net"

curl https://your-app-name.azurewebsites.net/swagger/docs/v1 | jq '.schemes'
# Output: ["https"]
```

## Environment Variables in Azure

### Automatic Variables (Set by Azure):
- `WEBSITE_HOSTNAME` - Your app's hostname (e.g., `your-app.azurewebsites.net`)
- `WEBSITE_SITE_NAME` - Your app service name
- `WEBSITE_INSTANCE_ID` - Instance identifier

### Manual Configuration (Optional):

If you need custom domains, add in Azure Portal ? Configuration ? Application Settings:

```
CUSTOM_API_DOMAIN = api.yourdomain.com
```

Then update SwaggerConfig:
```csharp
var customDomain = Environment.GetEnvironmentVariable("CUSTOM_API_DOMAIN");
if (!string.IsNullOrEmpty(customDomain))
{
    return $"https://{customDomain}";
}
```

## Vendor Extensions

The configuration also adds Azure-specific metadata:

```csharp
operation.vendorExtensions["x-azure-deployment"] = new
{
    hostname = websiteHostname,
    platform = "Azure App Service"
};
```

This appears in the OpenAPI spec as:
```json
{
  "paths": {
    "/api/products/{productId}/reviews": {
      "get": {
        "operationId": "GetProductReviews",
        "x-azure-deployment": {
 "hostname": "your-app-name.azurewebsites.net",
          "platform": "Azure App Service"
  }
      }
}
  }
}
```

## Benefits

### For Development:
? Works locally without configuration  
? Uses correct local port  
? HTTP for local testing  

### For Azure Deployment:
? Automatically detects Azure environment  
? Uses HTTPS by default  
? Correct hostname from Azure  
? No hardcoded URLs  

### For Azure AI Foundry:
? Agent knows exact API endpoint  
? Function calls go to correct server  
? HTTPS security enforced  
? Works with custom domains  

## Troubleshooting

### Issue: Swagger shows wrong URL on Azure

**Check:**
```bash
# SSH into Azure App Service
az webapp ssh --name your-app-name --resource-group your-rg

# Check environment variable
echo $WEBSITE_HOSTNAME
```

**Should return:** `your-app-name.azurewebsites.net`

### Issue: OpenAPI spec shows localhost on Azure

**Solution:** Restart the App Service:
```bash
az webapp restart --name your-app-name --resource-group your-rg
```

### Issue: Custom domain not showing

**Add to Web.config:**
```xml
<appSettings>
  <add key="CUSTOM_API_DOMAIN" value="api.yourdomain.com" />
</appSettings>
```

Update SwaggerConfig to read from config:
```csharp
var customDomain = System.Configuration.ConfigurationManager.AppSettings["CUSTOM_API_DOMAIN"];
```

## Summary

Your SwaggerConfig now:
- ? **Automatically detects Azure deployment** via `WEBSITE_HOSTNAME`
- ? **Uses correct server URL** (local or Azure)
- ? **Enforces HTTPS on Azure** for security
- ? **Compatible with Azure AI Foundry** Agent Service
- ? **Adds Azure metadata** to OpenAPI spec
- ? **No manual configuration needed** - works out of the box

When you deploy to Azure, the OpenAPI specification will automatically include the correct production server URL, and Azure AI Foundry will be able to discover and call your API functions correctly! ??
