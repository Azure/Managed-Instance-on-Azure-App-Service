using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

namespace devShop.Controllers
{
    /// <summary>
    /// Product Inventory API - Manage product stock and availability
    /// </summary>
    [RoutePrefix("api/inventory")]
    public class ProductInventoryController : ApiController
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(typeof(ProductInventoryController));

        private static readonly HashSet<string> SupportedSizes =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "XS", "S", "M", "L", "XL", "XXL", "XXXL"
            };

        /// <summary>
        /// Get all products in inventory with stock levels
        /// </summary>
        /// <returns>Complete list of all products with stock information</returns>
        /// <response code="200">Returns all products in inventory</response>
      /// <response code="500">Internal server error occurred</response>
        [HttpGet, Route("")]
        [ResponseType(typeof(List<ProductInventoryItem>))]
        [SwaggerOperation(operationId: "GetAllInventory", Tags = new[] { "ProductInventory" })]
     [SwaggerResponse(200, "Successfully retrieved inventory", typeof(List<ProductInventoryItem>))]
        [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult get_inventory()
        {
    try
 {
                var db = new ProductsDB();
  var inventory = db.GetAllInventory();
          return Ok(inventory);
       }
     catch (Exception ex)
          {
                return HandleServerError(ex);
     }
        }

    /// <summary>
        /// Get inventory details for a specific product
        /// </summary>
   /// <param name="item_id">The unique identifier of the product</param>
 /// <returns>Product inventory details including stock by size</returns>
        /// <response code="200">Returns inventory details for the product</response>
        /// <response code="404">Product not found</response>
  /// <response code="500">Internal server error occurred</response>
  [HttpGet, Route("product/{item_id:int}")]
        [ResponseType(typeof(ProductInventoryItem))]
        [SwaggerOperation(operationId: "GetProductInventory", Tags = new[] { "ProductInventory" })]
        [SwaggerResponse(200, "Successfully retrieved product inventory", typeof(ProductInventoryItem))]
     [SwaggerResponse(404, "Product not found")]
        [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult get_item_by_id(int item_id)
        {
  try
      {
   var db = new ProductsDB();
       var inventory = db.GetProductInventory(item_id);
    
  if (inventory == null)
    return NotFound();
    
     return Ok(inventory);
        }
  catch (Exception ex)
        {
        return HandleServerError(ex);
}
     }

        /// <summary>
        /// Get all products in a specific category with inventory
        /// </summary>
     /// <param name="categoryName">The name of the category (e.g., Jackets, Shirts)</param>
        /// <returns>List of products in the category with stock information</returns>
   /// <response code="200">Returns products in the category</response>
        /// <response code="400">Category name is required</response>
      /// <response code="500">Internal server error occurred</response>
        [HttpGet, Route("category/{categoryName}")]
        [ResponseType(typeof(List<ProductInventoryItem>))]
      [SwaggerOperation(operationId: "GetInventoryByCategory", Tags = new[] { "ProductInventory" })]
        [SwaggerResponse(200, "Successfully retrieved inventory for category", typeof(List<ProductInventoryItem>))]
        [SwaggerResponse(400, "Category name is required")]
        [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult GetByCategory(string categoryName)
        {
   if (string.IsNullOrWhiteSpace(categoryName))
     return BadRequest("Category name required");

   try
            {
    var db = new ProductsDB();
           var inventory = db.GetInventoryByCategory(categoryName);
         return Ok(inventory);
            }
 catch (Exception ex)
            {
          return HandleServerError(ex);
            }
        }

        /// <summary>
        /// Check stock availability for a specific product and size
        /// </summary>
    /// <param name="productId">The unique identifier of the product</param>
        /// <param name="size">Size code (XS, S, M, L, XL, XXL, XXXL)</param>
        /// <returns>Stock availability information</returns>
        /// <response code="200">Returns stock availability</response>
  /// <response code="400">Invalid size specified</response>
        /// <response code="404">Product not found</response>
   /// <response code="500">Internal server error occurred</response>
        [HttpGet, Route("product/{productId:int}/size/{size}")]
        [ResponseType(typeof(StockAvailability))]
  [SwaggerOperation(operationId: "CheckStockAvailability", Tags = new[] { "ProductInventory" })]
      [SwaggerResponse(200, "Successfully retrieved stock availability", typeof(StockAvailability))]
        [SwaggerResponse(400, "Invalid size specified")]
        [SwaggerResponse(404, "Product not found")]
        [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult CheckStock(int productId, string size)
        {
            var normalizedSize = NormalizeSize(size);
            if (normalizedSize == null)
                return BadRequest("Size must be one of: XS, S, M, L, XL, XXL, XXXL.");

      try
        {
         var db = new ProductsDB();
                var availability = db.CheckStockAvailability(productId, normalizedSize);
  
          if (availability == null)
            return NotFound();
        
        return Ok(availability);
            }
catch (Exception ex)
            {
       return HandleServerError(ex);
            }
   }

        /// <summary>
      /// Update stock quantity for a product size
        /// </summary>
        /// <param name="productId">The unique identifier of the product</param>
        /// <param name="request">Stock update details including size and quantity</param>
  /// <returns>Updated stock information</returns>
   /// <response code="200">Stock updated successfully</response>
   /// <response code="400">Invalid request data</response>
      /// <response code="404">Product not found</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpPut, Route("product/{productId:int}/stock")]
        [ApiKeyAuthorize]
        [ResponseType(typeof(StockUpdateResponse))]
  [SwaggerOperation(operationId: "UpdateProductStock", Tags = new[] { "ProductInventory" })]
        [SwaggerResponse(200, "Stock updated successfully", typeof(StockUpdateResponse))]
        [SwaggerResponse(400, "Invalid request data")]
        [SwaggerResponse(404, "Product not found")]
        [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult UpdateStock(int productId, [FromBody] StockUpdateRequest request)
        {
            if (request == null)
     return BadRequest("Request body required");
    
            var normalizedSize = NormalizeSize(request.Size);
            if (normalizedSize == null)
                return BadRequest("Size must be one of: XS, S, M, L, XL, XXL, XXXL.");

          if (request.Quantity < 0)
   return BadRequest("Quantity cannot be negative");

       try
            {
     var db = new ProductsDB();
    var result = db.UpdateProductStock(productId, normalizedSize, request.Quantity);
    
 if (result == null)
     return NotFound();
    
     return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleServerError(ex);
}
        }

        /// <summary>
        /// Get products with low stock (below threshold)
        /// </summary>
        /// <param name="threshold">Stock quantity threshold (default: 5)</param>
        /// <returns>List of products with low stock</returns>
        /// <response code="200">Returns products with low stock</response>
        /// <response code="500">Internal server error occurred</response>
      [HttpGet, Route("lowstock")]
      [ResponseType(typeof(List<LowStockItem>))]
        [SwaggerOperation(operationId: "GetLowStockProducts", Tags = new[] { "ProductInventory" })]
    [SwaggerResponse(200, "Successfully retrieved low stock products", typeof(List<LowStockItem>))]
        [SwaggerResponse(500, "Internal server error occurred")]
   public IHttpActionResult GetLowStock([FromUri] int threshold = 5)
        {
     try
    {
             var db = new ProductsDB();
           var lowStock = db.GetLowStockProducts(threshold);
    return Ok(lowStock);
  }
            catch (Exception ex)
            {
              return HandleServerError(ex);
          }
   }

        private static string NormalizeSize(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
                return null;

            var normalizedSize = size.Trim().ToUpperInvariant();
            return SupportedSizes.Contains(normalizedSize) ? normalizedSize : null;
        }

        private IHttpActionResult HandleServerError(Exception exception)
        {
            Log.Error("Inventory API request failed.", exception);
            return InternalServerError();
        }

      #region Request/Response Models

/// <summary>
        /// Product inventory item with stock details
        /// </summary>
        public class ProductInventoryItem
     {
          /// <summary>
        /// Product identifier
            /// </summary>
         /// <example>1</example>
      public int Id { get; set; }

     /// <summary>
            /// Product name
         /// </summary>
            /// <example>Navy Single-Breasted Slim Fit Formal Blazer</example>
         public string Name { get; set; }

        /// <summary>
     /// Product category
          /// </summary>
         /// <example>Jackets</example>
            public string Category { get; set; }

   /// <summary>
       /// Product price
        /// </summary>
            /// <example>89.99</example>
     public decimal Price { get; set; }

            /// <summary>
      /// Product description
     /// </summary>
  /// <example>Tailored navy blazer with notch lapels</example>
public string Description { get; set; }

            /// <summary>
       /// Stock quantities by size
     /// </summary>
            public Dictionary<string, int> Sizes { get; set; }

          /// <summary>
        /// Total stock across all sizes
     /// </summary>
          /// <example>57</example>
      public int TotalStock { get; set; }
        }

        /// <summary>
        /// Stock availability for a specific product and size
        /// </summary>
  public class StockAvailability
        {
      /// <summary>
            /// Product identifier
         /// </summary>
   public int ProductId { get; set; }

          /// <summary>
            /// Product name
          /// </summary>
            public string ProductName { get; set; }

    /// <summary>
        /// Size code
     /// </summary>
            /// <example>M</example>
public string Size { get; set; }

     /// <summary>
            /// Available quantity
            /// </summary>
            /// <example>12</example>
     public int Quantity { get; set; }

            /// <summary>
    /// Whether the item is in stock
     /// </summary>
    public bool IsInStock { get; set; }

 /// <summary>
      /// Stock status message
            /// </summary>
     /// <example>In Stock</example>
 public string Status { get; set; }
   }

        /// <summary>
        /// Request model for updating stock
        /// </summary>
        public class StockUpdateRequest
        {
/// <summary>
     /// Size code to update
     /// </summary>
            /// <example>M</example>
      public string Size { get; set; }

    /// <summary>
        /// New stock quantity
            /// </summary>
  /// <example>20</example>
     public int Quantity { get; set; }
 }

        /// <summary>
        /// Response model for stock update
        /// </summary>
        public class StockUpdateResponse
        {
            /// <summary>
       /// Product identifier
            /// </summary>
      public int ProductId { get; set; }

            /// <summary>
      /// Size that was updated
            /// </summary>
          public string Size { get; set; }

            /// <summary>
            /// Previous quantity
       /// </summary>
            public int PreviousQuantity { get; set; }

  /// <summary>
            /// New quantity
       /// </summary>
public int NewQuantity { get; set; }

       /// <summary>
            /// Update timestamp
            /// </summary>
      public DateTime UpdatedAt { get; set; }
        }

        /// <summary>
   /// Low stock item details
        /// </summary>
        public class LowStockItem
      {
         /// <summary>
       /// Product identifier
    /// </summary>
       public int ProductId { get; set; }

     /// <summary>
            /// Product name
/// </summary>
  public string ProductName { get; set; }

            /// <summary>
/// Category name
      /// </summary>
            public string Category { get; set; }

            /// <summary>
 /// Size with low stock
            /// </summary>
    public string Size { get; set; }

 /// <summary>
        /// Current quantity
            /// </summary>
    public int Quantity { get; set; }

          /// <summary>
            /// Alert level
    /// </summary>
 /// <example>Critical</example>
            public string AlertLevel { get; set; }
        }

 #endregion
    }
}
