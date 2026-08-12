# OpenAPI Operation IDs Reference

## Generated Operation IDs

| Endpoint | HTTP Method | Operation ID | Description |
|----------|-------------|--------------|-------------|
| `/api/products/{productId}/reviews` | GET | `ProductReviews_Get_GET` | Get reviews for specific product |
| `/api/products/{productId}/reviews` | POST | `ProductReviews_Post_POST` | Create new review for product |
| `/api/reviews` | GET | `ProductReviews_GetAll_GET` | Get all reviews (all products) |
| `/api/reviews/category/{categoryName}` | GET | `ProductReviews_GetByCategory_GET` | Get reviews by category |

## Server URLs

### Development:
```
http://localhost:6294
```

### Azure Production:
```
https://your-app-name.azurewebsites.net
```

## Usage in OpenAPI Clients

### JavaScript/TypeScript:
```javascript
// Using generated operation ID
const reviews = await client.ProductReviews_Get_GET({ productId: 5 });
```

### C# Client SDK:
```csharp
// Using operation ID
var reviews = await apiClient.ProductReviews_Get_GET(productId: 5);
```

### Python:
```python
# Using operation ID
reviews = client.product_reviews_get_get(product_id=5)
```

## Complete OpenAPI Specification

View at: `GET http://localhost:6294/swagger/docs/v1`

Key sections:
- `info` - API metadata
- `host` - Server host
- `schemes` - HTTP/HTTPS
- `paths` - All endpoints with operation IDs
- `definitions` - Request/response schemas
