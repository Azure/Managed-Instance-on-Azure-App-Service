using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;


namespace devShop
{

    //*******************************************************
    //
    // ProductDetails Class
    //
    // A simple data class that encapsulates details about
    // a particular product inside the devShop Product
    // database.
    //
    //*******************************************************

    public class ProductDetails
    {

        public String ProductName;
        public String ProductGender;
        public String ProductImage;
        public int ProductPrice;
        public String ProductDescription;
        public String ProductBrand;
        public int CatID;


    }

    public class CatDetails
    {

        public String CatName;
       


    }

    public class ProductReview
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ReviewerName { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public DateTime ReviewDate { get; set; }

        public string Product { get; set; }
        public string Category { get; set; }
    }

    //*******************************************************
    //
    // ProductsDB Class
    //
    // Business/Data Logic Class that encapsulates all data
    // logic necessary to query products within
    // the devShop Products database.
    //
    //*******************************************************

    public class ProductsDB
    {

        private static readonly IReadOnlyDictionary<string, string> SizeColumns =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "XS", "SizeXS" },
                { "S", "SizeS" },
                { "M", "SizeM" },
                { "L", "SizeL" },
                { "XL", "SizeXL" },
                { "XXL", "SizeXXL" },
                { "XXXL", "SizeXXXL" }
            };

        SqlConnection dbConnection;
        SqlCommand sqlCommand;
        DataTable allProducts;



        public ProductsDB()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("The DBConnection connection string is not configured.");

            dbConnection = new SqlConnection(connectionString);
            dbConnection.Open();
            sqlCommand = new SqlCommand("SELECT * FROM Products", dbConnection);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            allProducts = new DataTable();
            adapter.Fill(allProducts);
            sqlCommand.Dispose();



        }

        public DataTable GetProductCategories()
        {
            sqlCommand = new SqlCommand("SELECT * FROM Categories", dbConnection);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable categories = new DataTable();
            adapter.Fill(categories);
            sqlCommand.Dispose();
            return categories;
        }

        public DataTable GetProducts(int categoryID)
        {
            sqlCommand = new SqlCommand("SELECT * FROM products WHERE CategoryID = @categoryID", dbConnection);
            sqlCommand.Parameters.AddWithValue("@categoryID", categoryID);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable tmpDataTable = new DataTable();
            adapter.Fill(tmpDataTable);
            sqlCommand.Dispose();
            return tmpDataTable;
        }

        public DataTable GetAllProducts()
        {
            sqlCommand = new SqlCommand("SELECT * FROM products WHERE CategoryID IN (1,2,3)", dbConnection);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable tmpDataTable = new DataTable();
            adapter.Fill(tmpDataTable);
            sqlCommand.Dispose();
            return tmpDataTable;
        }
        public ProductDetails GetProductDetails(int productID)
        {
            sqlCommand = new SqlCommand("SELECT * FROM products WHERE productID = @productID", dbConnection);
            sqlCommand.Parameters.AddWithValue("@productID", productID);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable tmpDataTable = new DataTable();
            adapter.Fill(tmpDataTable);
            sqlCommand.Dispose();

            ProductDetails myProductDetails = new ProductDetails();
            if (tmpDataTable.Rows.Count > 0)
            {

                DataRow result = tmpDataTable.Rows[0];
                myProductDetails.ProductName = result["ProductName"].ToString();
                myProductDetails.ProductBrand = result["ProductBrand"].ToString();
                myProductDetails.ProductImage = result["ProductImage"].ToString();
                myProductDetails.ProductPrice = Int32.Parse(result["ProductPrice"].ToString());
                myProductDetails.ProductDescription = result["ProductDescription"].ToString().Trim();
                myProductDetails.CatID = Int32.Parse(result["CategoryID"].ToString());

            }
            return myProductDetails;
        }

        public CatDetails GetCategoryDetails(int categoryID)
        {
            sqlCommand = new SqlCommand("SELECT CategoryName FROM Categories WHERE CategoryID = @categoryID", dbConnection);
            sqlCommand.Parameters.Add("@categoryID", SqlDbType.Int).Value = categoryID;
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
            DataTable tmpDataTable = new DataTable();
            adapter.Fill(tmpDataTable);
            sqlCommand.Dispose();
            CatDetails myCatDetails = new CatDetails();
            if (tmpDataTable.Rows.Count > 0)
            {
                DataRow result = tmpDataTable.Rows[0];
                myCatDetails.CatName = result["CategoryName"].ToString();
            }
            return myCatDetails;
        }

        public void InsertOrderDetails(int productID, int quantity)
        {
            const string insertOrder = @"
INSERT INTO Orders ([ProductID], [ProductDesc], [CatID], [Quantity], [TotalPrice], [CatName])
SELECT p.ProductID, p.ProductName, p.CategoryID, @quantity, p.ProductPrice * @quantity, c.CategoryName
FROM Products p
INNER JOIN Categories c ON c.CategoryID = p.CategoryID
WHERE p.ProductID = @productID";

            using (var command = new SqlCommand(insertOrder, dbConnection))
            {
                command.Parameters.Add("@productID", SqlDbType.Int).Value = productID;
                command.Parameters.Add("@quantity", SqlDbType.Int).Value = quantity;
                if (command.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException("The selected product was not found.");
            }
        }

        public DataTable GetMostPopularProductsOfWeek()
        {

            Random r = new Random();
            var rowsTaken = new HashSet<int>();
            DataTable rndTable = allProducts.Clone();
            for (int i = 0; i < 3; i++)
            {
                int rndRowIndex = r.Next(allProducts.Rows.Count);
                while (!rowsTaken.Add(rndRowIndex))
                    rndRowIndex = r.Next(allProducts.Rows.Count);
                DataRow randomRow = allProducts.Rows[rndRowIndex];
                rndTable.ImportRow(randomRow);
            }
            return rndTable;

        }

        public int AddProductReview(int productId, string reviewerName, int rating, string comments)
        {
            // Lookup product to get CategoryID and ProductName
            int categoryId;
            string productName;
            using (var cmdMeta = new SqlCommand("SELECT CategoryID, ProductName FROM Products WHERE ProductID=@pid", dbConnection))
            {
                cmdMeta.Parameters.AddWithValue("@pid", productId);
                using (var rdr = cmdMeta.ExecuteReader())
                {
                    if (!rdr.Read())
                        throw new InvalidOperationException("Product not found for review.");
                    categoryId = rdr.GetInt32(0);
                    productName = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                }
            }

            string categoryName = null;
            using (var cmdCat = new SqlCommand("SELECT CategoryName FROM Categories WHERE CategoryID = @categoryId", dbConnection))
            {
                cmdCat.Parameters.Add("@categoryId", SqlDbType.Int).Value = categoryId;
                var result = cmdCat.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    categoryName = (string)result;
            }

            const string insertReview = @"
INSERT INTO ProductReviews (ProductID, ReviewerName, Rating, Comments, ReviewDate, Category, Product)
OUTPUT INSERTED.ReviewID
VALUES (@productId, @reviewerName, @rating, @comments, GETUTCDATE(), @categoryName, @productName)";
            using (var cmd = new SqlCommand(insertReview, dbConnection))
            {
                cmd.Parameters.Add("@productId", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@reviewerName", SqlDbType.NVarChar, 200).Value = (object)reviewerName ?? DBNull.Value;
                cmd.Parameters.Add("@rating", SqlDbType.Int).Value = rating;
                cmd.Parameters.Add("@comments", SqlDbType.NVarChar, 4000).Value = (object)comments ?? DBNull.Value;
                cmd.Parameters.Add("@categoryName", SqlDbType.NVarChar, 200).Value = (object)categoryName ?? DBNull.Value;
                cmd.Parameters.Add("@productName", SqlDbType.NVarChar, 500).Value = (object)productName ?? DBNull.Value;
                return (int)cmd.ExecuteScalar();
            }
        }

        public List<ProductReview> GetProductReviews(int productId)
        {
            var results = new List<ProductReview>();
            using (var cmd = new SqlCommand("SELECT ReviewID, ProductID, ReviewerName, Rating, Comments, ReviewDate,Category,Product FROM ProductReviews WHERE ProductID=@pid ORDER BY ReviewDate DESC", dbConnection))
            {
                cmd.Parameters.AddWithValue("@pid", productId);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        results.Add(new ProductReview
                        {
                            ReviewId = rdr.GetInt32(0),
                            ProductId = rdr.GetInt32(1),
                            ReviewerName = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                            Rating = rdr.GetInt32(3),
                            Comments = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                            ReviewDate = rdr.GetDateTime(5),
                            Category = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                            Product = rdr.IsDBNull(7) ? null : rdr.GetString(7)
                        });
                    }
                }
            }
            return results;
        }

        public List<ProductReview> GetAllProductReviews()
        {
            var results = new List<ProductReview>();
            using (var cmd = new SqlCommand("SELECT ReviewID, ProductID, ReviewerName, Rating, Comments, ReviewDate,Category,Product FROM ProductReviews ORDER BY ReviewDate DESC", dbConnection))
            {
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        results.Add(new ProductReview
                        {
                            ReviewId = rdr.GetInt32(0),
                            ProductId = rdr.GetInt32(1),
                            ReviewerName = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                            Rating = rdr.GetInt32(3),
                            Comments = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                            ReviewDate = rdr.GetDateTime(5),
                            Category = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                            Product = rdr.IsDBNull(7) ? null : rdr.GetString(7)

                        });
                    }
                }
            }
            return results;
        }

        public List<ProductReview> GetProductReviewsByCategoryName(string categoryName)
        {
            var results = new List<ProductReview>();
            using (var cmd = new SqlCommand("SELECT ReviewID, ProductID, ReviewerName, Rating, Comments, ReviewDate,Category,Product FROM ProductReviews WHERE Category=@cname ORDER BY ReviewDate DESC", dbConnection))
            {
                cmd.Parameters.AddWithValue("@cname", categoryName);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        results.Add(new ProductReview
                        {
                            ReviewId = rdr.GetInt32(0),
                            ProductId = rdr.GetInt32(1),
                            ReviewerName = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                            Rating = rdr.GetInt32(3),
                            Comments = rdr.IsDBNull(4) ? null : rdr.GetString(4),
                            ReviewDate = rdr.GetDateTime(5),
                            Category = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                            Product = rdr.IsDBNull(7) ? null : rdr.GetString(7)
                        });
                    }
                }
            }
            return results;
        }

        public void Dispose()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
            if (sqlCommand != null)
            {
                sqlCommand.Dispose();
            }
        }

      #region Inventory Management Methods

        /// <summary>
        /// Get all products with inventory information
        /// </summary>
        public List<Controllers.ProductInventoryController.ProductInventoryItem> GetAllInventory()
        {
            var inventory = new List<Controllers.ProductInventoryController.ProductInventoryItem>();
            
            using (var cmd = new SqlCommand(@"
     SELECT p.ProductID, p.ProductName, CAST(p.ProductPrice AS DECIMAL(18,2)) AS ProductPrice, 
          p.ProductDescription, c.CategoryName,
         ISNULL(i.SizeXS, 0) AS SizeXS, ISNULL(i.SizeS, 0) AS SizeS, 
     ISNULL(i.SizeM, 0) AS SizeM, ISNULL(i.SizeL, 0) AS SizeL,
         ISNULL(i.SizeXL, 0) AS SizeXL, ISNULL(i.SizeXXL, 0) AS SizeXXL, 
       ISNULL(i.SizeXXXL, 0) AS SizeXXXL
      FROM Products p
           LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
        LEFT JOIN ProductInventory i ON p.ProductID = i.ProductID
                WHERE i.ProductID IS NOT NULL
       ORDER BY p.ProductName", dbConnection))
         {
       using (var reader = cmd.ExecuteReader())
  {
       while (reader.Read())
    {
     var sizes = new Dictionary<string, int>
    {
       { "XS", reader.GetInt32(5) },
     { "S", reader.GetInt32(6) },
        { "M", reader.GetInt32(7) },
           { "L", reader.GetInt32(8) },
  { "XL", reader.GetInt32(9) },
  { "XXL", reader.GetInt32(10) },
            { "XXXL", reader.GetInt32(11) }
            };

  // Safely get price with proper handling
   decimal price = 0;
    if (!reader.IsDBNull(2))
               {
             try
               {
     price = reader.GetDecimal(2);
          }
      catch
                {
       // If it's stored as int, try that
    price = Convert.ToDecimal(reader.GetValue(2));
       }
    }

       inventory.Add(new Controllers.ProductInventoryController.ProductInventoryItem
       {
    Id = reader.GetInt32(0),
             Name = reader.IsDBNull(1) ? null : reader.GetString(1),
         Price = price,
    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
  Category = reader.IsDBNull(4) ? null : reader.GetString(4),
    Sizes = sizes,
TotalStock = sizes.Values.Sum()
     });
      }
    }
            }
         
         return inventory;
   }

        /// <summary>
 /// Get inventory for a specific product
 /// </summary>
      public Controllers.ProductInventoryController.ProductInventoryItem GetProductInventory(int productId)
        {
  using (var cmd = new SqlCommand(@"
  SELECT p.ProductID, p.ProductName, CAST(p.ProductPrice AS DECIMAL(18,2)) AS ProductPrice, 
  p.ProductDescription, c.CategoryName,
  ISNULL(i.SizeXS, 0) AS SizeXS, ISNULL(i.SizeS, 0) AS SizeS, 
          ISNULL(i.SizeM, 0) AS SizeM, ISNULL(i.SizeL, 0) AS SizeL,
     ISNULL(i.SizeXL, 0) AS SizeXL, ISNULL(i.SizeXXL, 0) AS SizeXXL, 
  ISNULL(i.SizeXXXL, 0) AS SizeXXXL
    FROM Products p
           LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
    LEFT JOIN ProductInventory i ON p.ProductID = i.ProductID
      WHERE p.ProductID = @productId", dbConnection))
            {
cmd.Parameters.AddWithValue("@productId", productId);
    
     using (var reader = cmd.ExecuteReader())
 {
    if (reader.Read())
        {
       var sizes = new Dictionary<string, int>
            {
    { "XS", reader.GetInt32(5) },
            { "S", reader.GetInt32(6) },
         { "M", reader.GetInt32(7) },
        { "L", reader.GetInt32(8) },
       { "XL", reader.GetInt32(9) },
       { "XXL", reader.GetInt32(10) },
            { "XXXL", reader.GetInt32(11) }
         };

          // Safely get price
     decimal price = 0;
       if (!reader.IsDBNull(2))
      {
         try
                  {
                price = reader.GetDecimal(2);
}
   catch
    {
          price = Convert.ToDecimal(reader.GetValue(2));
    }
 }

         return new Controllers.ProductInventoryController.ProductInventoryItem
{
       Id = reader.GetInt32(0),
  Name = reader.IsDBNull(1) ? null : reader.GetString(1),
    Price = price,
       Description = reader.IsDBNull(3) ? null : reader.GetString(3),
        Category = reader.IsDBNull(4) ? null : reader.GetString(4),
       Sizes = sizes,
                  TotalStock = sizes.Values.Sum()
         };
           }
        }
   }
       
    return null;
        }

     /// <summary>
        /// Get inventory by category
    /// </summary>
      public List<Controllers.ProductInventoryController.ProductInventoryItem> GetInventoryByCategory(string categoryName)
        {
    var inventory = new List<Controllers.ProductInventoryController.ProductInventoryItem>();
    
   using (var cmd = new SqlCommand(@"
     SELECT p.ProductID, p.ProductName, CAST(p.ProductPrice AS DECIMAL(18,2)) AS ProductPrice, 
          p.ProductDescription, c.CategoryName,
     ISNULL(i.SizeXS, 0) AS SizeXS, ISNULL(i.SizeS, 0) AS SizeS, 
      ISNULL(i.SizeM, 0) AS SizeM, ISNULL(i.SizeL, 0) AS SizeL,
         ISNULL(i.SizeXL, 0) AS SizeXL, ISNULL(i.SizeXXL, 0) AS SizeXXL, 
    ISNULL(i.SizeXXXL, 0) AS SizeXXXL
 FROM Products p
   LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
        LEFT JOIN ProductInventory i ON p.ProductID = i.ProductID
       WHERE c.CategoryName = @categoryName AND i.ProductID IS NOT NULL
      ORDER BY p.ProductName", dbConnection))
    {
           cmd.Parameters.AddWithValue("@categoryName", categoryName);
    
         using (var reader = cmd.ExecuteReader())
            {
 while (reader.Read())
  {
         var sizes = new Dictionary<string, int>
  {
              { "XS", reader.GetInt32(5) },
      { "S", reader.GetInt32(6) },
   { "M", reader.GetInt32(7) },
       { "L", reader.GetInt32(8) },
    { "XL", reader.GetInt32(9) },
    { "XXL", reader.GetInt32(10) },
 { "XXXL", reader.GetInt32(11) }
   };

   // Safely get price
  decimal price = 0;
  if (!reader.IsDBNull(2))
        {
           try
      {
         price = reader.GetDecimal(2);
    }
     catch
      {
       price = Convert.ToDecimal(reader.GetValue(2));
     }
 }

       inventory.Add(new Controllers.ProductInventoryController.ProductInventoryItem
      {
  Id = reader.GetInt32(0),
        Name = reader.IsDBNull(1) ? null : reader.GetString(1),
  Price = price,
        Description = reader.IsDBNull(3) ? null : reader.GetString(3),
      Category = reader.IsDBNull(4) ? null : reader.GetString(4),
   Sizes = sizes,
       TotalStock = sizes.Values.Sum()
       });
           }
          }
     }
   
        return inventory;
   }

  /// <summary>
        /// Check stock availability for a product size
        /// </summary>
 public Controllers.ProductInventoryController.StockAvailability CheckStockAvailability(int productId, string size)
       {
           string sizeColumn;
           if (!SizeColumns.TryGetValue(size ?? string.Empty, out sizeColumn))
               throw new ArgumentOutOfRangeException(nameof(size), "Unsupported product size.");

       using (var cmd = new SqlCommand($@"
         SELECT p.ProductID, p.ProductName, ISNULL(i.{sizeColumn}, 0) AS Quantity
     FROM Products p
    LEFT JOIN ProductInventory i ON p.ProductID = i.ProductID
    WHERE p.ProductID = @productId", dbConnection))
       {
    cmd.Parameters.AddWithValue("@productId", productId);
    
             using (var reader = cmd.ExecuteReader())
    {
                if (reader.Read())
        {
    var quantity = reader.GetInt32(2);
        var isInStock = quantity > 0;
       
    return new Controllers.ProductInventoryController.StockAvailability
             {
    ProductId = reader.GetInt32(0),
   ProductName = reader.IsDBNull(1) ? null : reader.GetString(1),
   Size = size,
                  Quantity = quantity,
                IsInStock = isInStock,
     Status = isInStock ? (quantity < 5 ? "Low Stock" : "In Stock") : "Out of Stock"
 };
           }
       }
      }
        
   return null;
        }

        /// <summary>
        /// Update product stock for a specific size
        /// </summary>
        public Controllers.ProductInventoryController.StockUpdateResponse UpdateProductStock(int productId, string size, int quantity)
        {
            string sizeColumn;
            if (!SizeColumns.TryGetValue(size ?? string.Empty, out sizeColumn))
                throw new ArgumentOutOfRangeException(nameof(size), "Unsupported product size.");

     // Get current quantity
    int currentQuantity = 0;
        using (var cmd = new SqlCommand($"SELECT ISNULL({sizeColumn}, 0) FROM ProductInventory WHERE ProductID = @productId", dbConnection))
            {
 cmd.Parameters.AddWithValue("@productId", productId);
      var result = cmd.ExecuteScalar();
        if (result != null && result != DBNull.Value)
                  currentQuantity = Convert.ToInt32(result);
            }
  
          // Update or insert inventory record
   using (var cmd = new SqlCommand($@"
         IF EXISTS (SELECT 1 FROM ProductInventory WHERE ProductID = @productId)
          UPDATE ProductInventory SET {sizeColumn} = @quantity WHERE ProductID = @productId
     ELSE
       INSERT INTO ProductInventory (ProductID, {sizeColumn}) VALUES (@productId, @quantity)", 
      dbConnection))
     {
         cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@quantity", quantity);
             cmd.ExecuteNonQuery();
      }
            
      return new Controllers.ProductInventoryController.StockUpdateResponse
            {
     ProductId = productId,
  Size = size,
           PreviousQuantity = currentQuantity,
                NewQuantity = quantity,
             UpdatedAt = DateTime.UtcNow
            };
  }

  /// <summary>
        /// Get products with low stock
  /// </summary>
        public List<Controllers.ProductInventoryController.LowStockItem> GetLowStockProducts(int threshold)
    {
            var lowStock = new List<Controllers.ProductInventoryController.LowStockItem>();
            
        using (var cmd = new SqlCommand(@"
           SELECT p.ProductID, p.ProductName, c.CategoryName, 'XS' AS Size, i.SizeXS AS Quantity
      FROM Products p
          INNER JOIN Categories c ON p.CategoryID = c.CategoryID
     INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
      WHERE i.SizeXS > 0 AND i.SizeXS <= @threshold
       UNION ALL
       SELECT p.ProductID, p.ProductName, c.CategoryName, 'S', i.SizeS
        FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
          WHERE i.SizeS > 0 AND i.SizeS <= @threshold
       UNION ALL
             SELECT p.ProductID, p.ProductName, c.CategoryName, 'M', i.SizeM
            FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID
  INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
            WHERE i.SizeM > 0 AND i.SizeM <= @threshold
     UNION ALL
          SELECT p.ProductID, p.ProductName, c.CategoryName, 'L', i.SizeL
              FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID
         INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
    WHERE i.SizeL > 0 AND i.SizeL <= @threshold
                UNION ALL
         SELECT p.ProductID, p.ProductName, c.CategoryName, 'XL', i.SizeXL
     FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID
   INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
   WHERE i.SizeXL > 0 AND i.SizeXL <= @threshold
   UNION ALL
     SELECT p.ProductID, p.ProductName, c.CategoryName, 'XXL', i.SizeXXL
 FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID
                INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
     WHERE i.SizeXXL > 0 AND i.SizeXXL <= @threshold
        UNION ALL
          SELECT p.ProductID, p.ProductName, c.CategoryName, 'XXXL', i.SizeXXXL
                FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID
   INNER JOIN ProductInventory i ON p.ProductID = i.ProductID
   WHERE i.SizeXXXL > 0 AND i.SizeXXXL <= @threshold
 ORDER BY Quantity ASC", dbConnection))
            {
                cmd.Parameters.AddWithValue("@threshold", threshold);
          
using (var reader = cmd.ExecuteReader())
            {
        while (reader.Read())
{
     var quantity = reader.GetInt32(4);
        lowStock.Add(new Controllers.ProductInventoryController.LowStockItem
      {
    ProductId = reader.GetInt32(0),
 ProductName = reader.IsDBNull(1) ? null : reader.GetString(1),
            Category = reader.IsDBNull(2) ? null : reader.GetString(2),
  Size = reader.GetString(3),
     Quantity = quantity,
     AlertLevel = quantity <= 2 ? "Critical" : quantity <= threshold / 2 ? "Warning" : "Low"
         });
         }
    }
     }
      
          return lowStock;
    }

        #endregion
    }
}