using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger.Annotations;

namespace devShop.Controllers
{
    /// <summary>
    /// Product Reviews API - Manage product reviews and ratings
    /// </summary>
    [RoutePrefix("api/products/{productId:int}/reviews")]
    public class ProductReviewsController : ApiController
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(typeof(ProductReviewsController));

        /// <summary>
 /// Get all reviews for a specific product
        /// </summary>
      /// <param name="productId">The unique identifier of the product</param>
  /// <returns>List of product reviews</returns>
        /// <response code="200">Returns the list of reviews for the specified product</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpGet, Route("")]
        [ResponseType(typeof(List<ProductReview>))]
        [SwaggerOperation(operationId: "GetProductReviews", Tags = new[] { "ProductReviews" })]
   [SwaggerResponse(200, "Successfully retrieved product reviews", typeof(List<ProductReview>))]
   [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult Get(int productId)
        {
 try
     {
         var db = new ProductsDB();
       var reviews = db.GetProductReviews(productId);
              return Ok(reviews);
            }
            catch (Exception ex)
   {
    return HandleServerError(ex);
            }
        }

 /// <summary>
      /// Request model for creating a new product review
        /// </summary>
     public class CreateReviewRequest
        {
          /// <summary>
   /// Name of the reviewer
       /// </summary>
            /// <example>John Doe</example>
    public string ReviewerName { get; set; }

   /// <summary>
       /// Rating score from 1 to 5 stars
 /// </summary>
     /// <example>5</example>
     public int Rating { get; set; } // 1-5

 /// <summary>
   /// Review comments and feedback
 /// </summary>
  /// <example>Great product! Highly recommended.</example>
public string Comments { get; set; }
        }

     /// <summary>
        /// Create a new review for a product
        /// </summary>
        /// <param name="productId">The unique identifier of the product being reviewed</param>
        /// <param name="request">Review details including reviewer name, rating, and comments</param>
        /// <returns>Created review with new review ID</returns>
        /// <response code="201">Review created successfully</response>
        /// <response code="400">Invalid request - missing required fields or invalid rating</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpPost, Route("")]
        [ApiKeyAuthorize]
        [ResponseType(typeof(object))]
        [SwaggerOperation(operationId: "CreateProductReview", Tags = new[] { "ProductReviews" })]
        [SwaggerResponse(201, "Review created successfully", typeof(object))]
        [SwaggerResponse(400, "Invalid request - missing required fields or invalid rating")]
 [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult Post(int productId, [FromBody] CreateReviewRequest request)
        {
            if (request == null)
     return BadRequest("Request body required");
            if (string.IsNullOrWhiteSpace(request.ReviewerName) || request.ReviewerName.Length > 200)
                return BadRequest("Reviewer name is required and must not exceed 200 characters.");
            if (string.IsNullOrWhiteSpace(request.Comments) || request.Comments.Length > 4000)
                return BadRequest("Comments are required and must not exceed 4000 characters.");
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");
     try
         {
    var db = new ProductsDB();
    int newId = db.AddProductReview(productId, request.ReviewerName, request.Rating, request.Comments);
      return Created($"/api/products/{productId}/reviews/{newId}", new { ReviewId = newId });
            }
            catch (Exception ex)
 {
       return HandleServerError(ex);
   }
        }

        /// <summary>
     /// Get all product reviews across all products
        /// </summary>
        /// <returns>Complete list of all product reviews in the system</returns>
        /// <response code="200">Returns all product reviews</response>
     /// <response code="500">Internal server error occurred</response>
      [HttpGet, Route("~/api/reviews")]
        [ResponseType(typeof(List<ProductReview>))]
        [SwaggerOperation(operationId: "GetAllReviews", Tags = new[] { "ProductReviews" })]
        [SwaggerResponse(200, "Successfully retrieved all reviews", typeof(List<ProductReview>))]
        [SwaggerResponse(500, "Internal server error occurred")]
     public IHttpActionResult GetAll()
      {
   try
          {
      var db = new ProductsDB();
                var reviews = db.GetAllProductReviews();
return Ok(reviews);
            }
       catch (Exception ex)
            {
     return HandleServerError(ex);
      }
        }

        /// <summary>
        /// Get all reviews filtered by category name
        /// </summary>
 /// <param name="categoryName">The name of the category to filter reviews (e.g., Electronics, Clothing, Books)</param>
   /// <returns>List of reviews for products in the specified category</returns>
     /// <response code="200">Returns reviews for the specified category</response>
        /// <response code="400">Category name is required</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpGet, Route("~/api/reviews/category/{categoryName}")]
      [ResponseType(typeof(List<ProductReview>))]
        [SwaggerOperation(operationId: "GetReviewsByCategory", Tags = new[] { "ProductReviews" })]
        [SwaggerResponse(200, "Successfully retrieved reviews for the category", typeof(List<ProductReview>))]
        [SwaggerResponse(400, "Category name is required")]
        [SwaggerResponse(500, "Internal server error occurred")]
        public IHttpActionResult GetByCategory(string categoryName)
        {
         if (string.IsNullOrWhiteSpace(categoryName))
          return BadRequest("Category name required");
   try
 {
          var db = new ProductsDB();
        var reviews = db.GetProductReviewsByCategoryName(categoryName);
         return Ok(reviews);
 }
 catch (Exception ex)
     {
       return HandleServerError(ex);
   }
    }

        private IHttpActionResult HandleServerError(Exception exception)
        {
            Log.Error("Product reviews API request failed.", exception);
            return InternalServerError();
        }
    }
}
