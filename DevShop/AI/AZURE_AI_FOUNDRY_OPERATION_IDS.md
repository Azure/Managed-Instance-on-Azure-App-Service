# Azure AI Foundry Agent Service - Operation IDs

## ? SwaggerOperation Attributes Added

All methods in `ProductReviewsController.cs` now have explicit `operationId` properties via `[SwaggerOperation]` attributes, making them fully compatible with **Azure AI Foundry Agent Service**.

## Operation IDs for Azure AI Foundry

| Endpoint | HTTP Method | Operation ID | Tags | Description |
|----------|-------------|--------------|------|-------------|
| `/api/products/{productId}/reviews` | GET | **GetProductReviews** | ProductReviews | Get all reviews for a specific product |
| `/api/products/{productId}/reviews` | POST | **CreateProductReview** | ProductReviews | Create a new review for a product |
| `/api/reviews` | GET | **GetAllReviews** | ProductReviews | Get all product reviews across all products |
| `/api/reviews/category/{categoryName}` | GET | **GetReviewsByCategory** | ProductReviews | Get reviews filtered by category name |

## Code Changes

### Added to ProductReviewsController.cs:

```csharp
using Swashbuckle.Swagger.Annotations;

// Example for each method:
[SwaggerOperation(operationId: "GetProductReviews", Tags = new[] { "ProductReviews" })]
[SwaggerResponse(200, "Successfully retrieved product reviews", typeof(List<ProductReview>))]
[SwaggerResponse(500, "Internal server error occurred")]
public IHttpActionResult Get(int productId)
{
// ... existing code
}
```

### Updated SwaggerConfig.cs:

```csharp
// Enable Swagger annotations for operationId support
c.EnableAnnotations();
```

## Azure AI Foundry Agent Configuration

### Function Definitions

When creating your agent in Azure AI Foundry, the functions will be automatically discovered with these operation IDs:

```json
{
  "functions": [
    {
      "name": "GetProductReviews",
      "description": "Get all reviews for a specific product",
   "parameters": {
        "type": "object",
     "properties": {
          "productId": {
    "type": "integer",
    "description": "The unique identifier of the product"
          }
        },
        "required": ["productId"]
      }
    },
    {
      "name": "CreateProductReview",
      "description": "Create a new review for a product",
      "parameters": {
"type": "object",
     "properties": {
       "productId": {
            "type": "integer",
 "description": "The unique identifier of the product being reviewed"
      },
    "request": {
     "type": "object",
     "properties": {
            "reviewerName": {
       "type": "string",
       "description": "Name of the reviewer"
     },
     "rating": {
                "type": "integer",
     "description": "Rating score from 1 to 5 stars",
   "minimum": 1,
    "maximum": 5
            },
   "comments": {
    "type": "string",
   "description": "Review comments and feedback"
              }
   },
            "required": ["rating"]
    }
        },
        "required": ["productId", "request"]
      }
    },
    {
      "name": "GetAllReviews",
      "description": "Get all product reviews across all products",
      "parameters": {
        "type": "object",
        "properties": {}
      }
    },
    {
      "name": "GetReviewsByCategory",
   "description": "Get all reviews filtered by category name",
      "parameters": {
    "type": "object",
      "properties": {
          "categoryName": {
            "type": "string",
     "description": "The name of the category to filter reviews"
          }
    },
     "required": ["categoryName"]
      }
    }
  ]
}
```

## OpenAPI/Swagger Schema Output

The generated OpenAPI specification will now include explicit operation IDs:

```json
{
  "swagger": "2.0",
  "info": {
    "version": "v1",
    "title": "devShop Product Reviews API",
    "description": "REST API for managing product reviews and ratings"
  },
  "host": "localhost:6294",
  "schemes": ["http"],
  "paths": {
    "/api/products/{productId}/reviews": {
      "get": {
  "tags": ["ProductReviews"],
        "summary": "Get all reviews for a specific product",
        "operationId": "GetProductReviews",
        "consumes": [],
        "produces": ["application/json", "text/json"],
 "parameters": [
  {
            "name": "productId",
        "in": "path",
    "description": "The unique identifier of the product",
      "required": true,
         "type": "integer",
 "format": "int32"
      }
        ],
        "responses": {
       "200": {
       "description": "Successfully retrieved product reviews",
  "schema": {
         "type": "array",
       "items": {
"$ref": "#/definitions/ProductReview"
   }
 }
          },
          "500": {
            "description": "Internal server error occurred"
          }
        }
      },
      "post": {
        "tags": ["ProductReviews"],
        "summary": "Create a new review for a product",
      "operationId": "CreateProductReview",
        "consumes": ["application/json", "text/json"],
        "produces": ["application/json", "text/json"],
        "parameters": [
 {
 "name": "productId",
    "in": "path",
        "description": "The unique identifier of the product being reviewed",
  "required": true,
            "type": "integer",
        "format": "int32"
          },
          {
  "name": "request",
         "in": "body",
            "description": "Review details including reviewer name, rating, and comments",
          "required": true,
 "schema": {
  "$ref": "#/definitions/CreateReviewRequest"
     }
          }
        ],
        "responses": {
          "201": {
 "description": "Review created successfully",
            "schema": {
              "type": "object"
    }
          },
          "400": {
          "description": "Invalid request - missing required fields or invalid rating"
          },
     "500": {
"description": "Internal server error occurred"
          }
 }
      }
  },
    "/api/reviews": {
      "get": {
        "tags": ["ProductReviews"],
        "summary": "Get all product reviews across all products",
 "operationId": "GetAllReviews",
        "consumes": [],
  "produces": ["application/json", "text/json"],
   "responses": {
          "200": {
  "description": "Successfully retrieved all reviews",
      "schema": {
       "type": "array",
  "items": {
      "$ref": "#/definitions/ProductReview"
           }
            }
      },
   "500": {
       "description": "Internal server error occurred"
    }
        }
      }
    },
    "/api/reviews/category/{categoryName}": {
      "get": {
"tags": ["ProductReviews"],
        "summary": "Get all reviews filtered by category name",
        "operationId": "GetReviewsByCategory",
        "consumes": [],
    "produces": ["application/json", "text/json"],
        "parameters": [
          {
            "name": "categoryName",
         "in": "path",
        "description": "The name of the category to filter reviews",
            "required": true,
    "type": "string"
          }
 ],
"responses": {
     "200": {
            "description": "Successfully retrieved reviews for the category",
  "schema": {
"type": "array",
            "items": {
    "$ref": "#/definitions/ProductReview"
    }
   }
    },
          "400": {
            "description": "Category name is required"
          },
 "500": {
   "description": "Internal server error occurred"
          }
        }
      }
    }
  }
}
```

## Using in Azure AI Foundry

### 1. Deploy Your API
Deploy to Azure App Service as documented in `AI/DEPLOYMENT_GUIDE.md`

### 2. Get OpenAPI Specification
```bash
curl https://your-app.azurewebsites.net/swagger/docs/v1 > openapi.json
```

### 3. Import to Azure AI Foundry

In Azure AI Foundry:
1. Go to **Agents** ? **Create Agent**
2. Click **Add Functions**
3. Select **Import from OpenAPI**
4. Upload your `openapi.json` or provide the URL:
   ```
   https://your-app.azurewebsites.net/swagger/docs/v1
   ```

### 4. Azure AI Foundry Will Auto-Detect:
- ? Operation IDs: `GetProductReviews`, `CreateProductReview`, `GetAllReviews`, `GetReviewsByCategory`
- ? Parameters and their types
- ? Request/response schemas
- ? Descriptions and examples
- ? Tags for grouping

## Example Agent Conversation

**User:** "Show me reviews for product 123"

**Agent:** *Calls `GetProductReviews` with `productId: 123`*

**Response:**
```json
[
  {
    "reviewId": 1,
  "productId": 123,
    "reviewerName": "John Doe",
    "rating": 5,
    "comments": "Excellent product!",
    "reviewDate": "2025-01-28T10:30:00Z",
    "product": "Laptop",
    "category": "Electronics"
  }
]
```

**Agent:** "I found 1 review for product 123. John Doe gave it 5 stars and said 'Excellent product!'"

---

**User:** "Add a 4-star review for product 456"

**Agent:** *Asks for details then calls `CreateProductReview`*
```json
{
  "productId": 456,
  "request": {
    "reviewerName": "User",
    "rating": 4,
    "comments": "Good product"
  }
}
```

**Agent:** "I've successfully added your 4-star review for product 456!"

## Testing

### Verify Operation IDs in Swagger UI:
1. Navigate to: `http://localhost:6294/swagger`
2. Expand any endpoint
3. Look for **"operationId"** in the schema
4. Verify it matches the table above

### Test in Azure AI Foundry Playground:
After importing to Azure AI Foundry:
- "Get all reviews"
- "Show me electronics reviews"
- "Add a 5-star review for product 10"
- "What are the reviews for product 5?"

## Benefits of Explicit Operation IDs

? **Predictable Function Names** in Azure AI Foundry
? **Better AI Agent Understanding** with semantic names
? **Easier Debugging** with consistent identifiers
? **Auto-Generated Client SDKs** use operation IDs
? **Version Compatibility** - IDs won't change with code refactoring

## Summary

Your API is now **fully compatible with Azure AI Foundry Agent Service** with:
- ? Explicit `operationId` properties via `[SwaggerOperation]`
- ? Comprehensive `[SwaggerResponse]` attributes
- ? Semantic operation names for AI agents
- ? Complete OpenAPI/Swagger documentation
- ? Ready for Azure deployment

**Next Step:** Deploy to Azure and import into AI Foundry! ??
