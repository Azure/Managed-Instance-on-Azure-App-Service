# Azure AI Foundry - Complete Deployment Checklist

## ? Pre-Deployment Verification

### 1. Code Changes Complete
- ? ProductReviewsController has `[SwaggerOperation]` attributes with explicit operationIds
- ? SwaggerConfig has `c.EnableAnnotations()` enabled
- ? SwaggerConfig detects `WEBSITE_HOSTNAME` for Azure server URL
- ? All methods have `[SwaggerResponse]` attributes
- ? XML documentation comments added
- ? Build succeeds with no errors

### 2. Local Testing
```bash
# Test Swagger UI
http://localhost:6294/swagger

# Get OpenAPI JSON
curl http://localhost:6294/swagger/docs/v1 > openapi-local.json

# Verify operation IDs exist
cat openapi-local.json | jq '.paths[].get.operationId'
# Should show: GetProductReviews, GetAllReviews, GetReviewsByCategory

cat openapi-local.json | jq '.paths[].post.operationId'
# Should show: CreateProductReview

# Verify server URL (local)
cat openapi-local.json | jq '.host'
# Should show: "localhost:6294"
```

## ?? Azure Deployment

### Step 1: Deploy to Azure App Service

#### Option A: Visual Studio
1. Right-click `devShop` project
2. Click **Publish**
3. Target: **Azure** ? **Azure App Service (Windows)**
4. Create new or select existing App Service
5. Configuration:
   - Name: `your-app-name`
   - Resource Group: `rg-devshop-api`
   - Hosting Plan: B1 or higher
   - Region: East US (or preferred)
6. Click **Publish**

#### Option B: Azure CLI
```bash
# Login
az login

# Create resource group
az group create --name rg-devshop-api --location eastus

# Create App Service Plan
az appservice plan create \
  --name plan-devshop-api \
  --resource-group rg-devshop-api \
  --sku B1

# Create Web App
az webapp create \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --plan plan-devshop-api \
  --runtime "ASPNET:V4.8"

# Deploy (from project directory)
az webapp deployment source config-zip \
  --resource-group rg-devshop-api \
  --name your-app-name \
  --src devShop.zip
```

### Step 2: Configure Azure App Service

#### Enable HTTPS Only:
```bash
az webapp update \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --set httpsOnly=true
```

#### Configure Connection String:
```bash
# Add database connection string
az webapp config connection-string set \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --connection-string-type SQLAzure \
  --settings ConnectionString="Server=your-server.database.windows.net;Database=devShop;User Id=user;Password=pass;Encrypt=true;"
```

#### Add Application Settings (Optional):
```bash
az webapp config appsettings set \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --settings \
    WEBSITE_LOAD_USER_PROFILE=1 \
    ASPNETCORE_ENVIRONMENT=Production
```

### Step 3: Verify Azure Deployment

```bash
# Get deployment URL
az webapp show \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --query defaultHostName \
  --output tsv
# Output: your-app-name.azurewebsites.net

# Test health endpoint
curl https://your-app-name.azurewebsites.net/api/agent/health

# Test Swagger UI
https://your-app-name.azurewebsites.net/swagger

# Get OpenAPI JSON
curl https://your-app-name.azurewebsites.net/swagger/docs/v1 > openapi-azure.json

# Verify server URL (should be Azure)
cat openapi-azure.json | jq '.host'
# Should show: "your-app-name.azurewebsites.net"

cat openapi-azure.json | jq '.schemes'
# Should show: ["https"]

# Verify operation IDs
cat openapi-azure.json | jq '.paths | to_entries[] | {path: .key, methods: .value | keys}'
```

## ?? Azure AI Foundry Configuration

### Step 1: Access Azure AI Foundry
```bash
# Open Azure AI Foundry
https://ai.azure.com
```

### Step 2: Create Agent

1. Navigate to **Agents** ? **Create Agent**
2. Fill in details:
   - **Name:** `ProductReviewsAgent`
   - **Description:** "AI agent for managing product reviews and ratings"
   - **Instructions:**
     ```
     You are a helpful assistant that manages product reviews.
     You can:
 1. Retrieve reviews for specific products
     2. Get all reviews across all products
     3. Filter reviews by category
4. Create new product reviews
     
     Always validate inputs and provide clear, concise responses.
     ```

### Step 3: Import Functions from OpenAPI

#### Method 1: URL Import (Recommended)
```
OpenAPI Spec URL: https://your-app-name.azurewebsites.net/swagger/docs/v1
```

Click **Import** ? Azure AI Foundry will auto-discover:
- ? `GetProductReviews` (productId: int)
- ? `CreateProductReview` (productId: int, request: object)
- ? `GetAllReviews` ()
- ? `GetReviewsByCategory` (categoryName: string)

#### Method 2: Manual Import
1. Download OpenAPI spec:
   ```bash
   curl https://your-app-name.azurewebsites.net/swagger/docs/v1 > openapi.json
   ```
2. In AI Foundry: **Import from File** ? Upload `openapi.json`

### Step 4: Configure Function Endpoint

Set the base endpoint for function calls:
```
https://your-app-name.azurewebsites.net
```

Or use the AgentController endpoint:
```
https://your-app-name.azurewebsites.net/api/agent/function
```

### Step 5: Test in Playground

Try these prompts:

#### Test 1: Get All Reviews
```
User: "Show me all product reviews"
Expected: Agent calls GetAllReviews() ? Returns JSON ? Formats response
```

#### Test 2: Get Reviews for Product
```
User: "Get reviews for product ID 5"
Expected: Agent calls GetProductReviews(5) ? Returns reviews for product 5
```

#### Test 3: Filter by Category
```
User: "Show me all electronics reviews"
Expected: Agent calls GetReviewsByCategory("Electronics") ? Returns filtered reviews
```

#### Test 4: Create Review
```
User: "Add a 5-star review for product 10 saying 'Great product!'"
Expected: Agent calls CreateProductReview(10, {rating: 5, comments: "Great product!"})
```

## ?? Validation Tests

### API Endpoint Tests
```bash
# Test GetProductReviews
curl https://your-app-name.azurewebsites.net/api/products/1/reviews

# Test GetAllReviews
curl https://your-app-name.azurewebsites.net/api/reviews

# Test GetReviewsByCategory
curl https://your-app-name.azurewebsites.net/api/reviews/category/Electronics

# Test CreateProductReview
curl -X POST https://your-app-name.azurewebsites.net/api/products/1/reviews \
  -H "Content-Type: application/json" \
  -d '{"reviewerName":"Test User","rating":5,"comments":"Test review"}'
```

### Agent Function Tests
```bash
# Test via AgentController
curl -X POST https://your-app-name.azurewebsites.net/api/agent/function \
  -H "Content-Type: application/json" \
  -d '{
    "function_name": "GetAllReviews",
    "arguments": {}
  }'

curl -X POST https://your-app-name.azurewebsites.net/api/agent/function \
  -H "Content-Type: application/json" \
  -d '{
  "function_name": "GetProductReviews",
    "arguments": {"productId": 5}
  }'
```

## ?? Monitoring & Troubleshooting

### Enable Application Insights
```bash
# Create Application Insights
az monitor app-insights component create \
  --app insights-devshop-api \
  --location eastus \
  --resource-group rg-devshop-api

# Link to Web App
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app insights-devshop-api \
  --resource-group rg-devshop-api \
  --query instrumentationKey -o tsv)

az webapp config appsettings set \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY
```

### Check Logs
```bash
# Stream logs
az webapp log tail \
  --name your-app-name \
  --resource-group rg-devshop-api

# Download logs
az webapp log download \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --log-file logs.zip
```

### Common Issues

#### Issue: Wrong Server URL in OpenAPI
**Check:**
```bash
az webapp config appsettings list \
  --name your-app-name \
  --resource-group rg-devshop-api \
  --query "[?name=='WEBSITE_HOSTNAME'].value" -o tsv
```

**Should return:** `your-app-name.azurewebsites.net`

#### Issue: Functions Not Discovered by AI Foundry
**Verify:**
1. OpenAPI spec has explicit `operationId` fields
2. Server URL is correct (HTTPS)
3. Endpoints are accessible publicly

**Test:**
```bash
curl https://your-app-name.azurewebsites.net/swagger/docs/v1 | \
  jq '.paths | to_entries[] | .value | to_entries[] | select(.value.operationId) | .value.operationId'
```

Should output:
```
GetProductReviews
CreateProductReview
GetAllReviews
GetReviewsByCategory
```

## ? Final Checklist

- [ ] Application deployed to Azure App Service
- [ ] HTTPS enabled
- [ ] Connection strings configured
- [ ] Swagger UI accessible: `https://your-app-name.azurewebsites.net/swagger`
- [ ] OpenAPI JSON accessible: `https://your-app-name.azurewebsites.net/swagger/docs/v1`
- [ ] Server URL in OpenAPI spec shows Azure hostname (not localhost)
- [ ] All 4 operation IDs present in OpenAPI spec
- [ ] Agent created in Azure AI Foundry
- [ ] Functions imported from OpenAPI URL
- [ ] Agent tested in Playground
- [ ] All 4 test prompts working correctly
- [ ] Application Insights enabled (optional)
- [ ] Logs streaming enabled

## ?? Success Criteria

Your deployment is successful when:

1. ? **Swagger UI loads** at `https://your-app-name.azurewebsites.net/swagger`
2. ? **OpenAPI spec** shows Azure hostname in `host` field
3. ? **All 4 operation IDs** present: GetProductReviews, CreateProductReview, GetAllReviews, GetReviewsByCategory
4. ? **Azure AI Foundry** successfully imports all 4 functions
5. ? **Agent responds** to test prompts correctly
6. ? **Function calls** execute successfully from agent

## ?? Reference Documentation

- [AZURE_AI_FOUNDRY_OPERATION_IDS.md](./AZURE_AI_FOUNDRY_OPERATION_IDS.md) - Complete operation ID reference
- [AZURE_SERVER_URL_CONFIG.md](./AZURE_SERVER_URL_CONFIG.md) - Server URL configuration details
- [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) - Full deployment instructions
- [DEPLOYMENT_CHECKLIST.md](./DEPLOYMENT_CHECKLIST.md) - Original checklist

---

**Your API is now fully deployed and integrated with Azure AI Foundry!** ??
