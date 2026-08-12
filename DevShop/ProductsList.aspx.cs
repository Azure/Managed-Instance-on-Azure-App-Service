using System;
using System.Data;
using System.Text;


namespace devShop
{
    public partial class ProductsList : System.Web.UI.Page
    {

        private static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(ProductsList));
        public ProductsList()
        {
            Page.Init += new System.EventHandler(Page_Init);
        }
        protected DataTable catProductList;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Log.Debug("Page_Load: Loading product list page");
                // Obtain categoryId from QueryString
                int categoryId = Int32.Parse(Request.Params["CategoryID"]);

                // Obtain products 
                devShop.ProductsDB productCatalogue = new devShop.ProductsDB();

                catProductList = productCatalogue.GetProducts(categoryId);
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < catProductList.Rows.Count; i++)
                {
                    DataRow row = catProductList.Rows[i];
                    sb.Append("<div class=\"col\">");
                    sb.Append("<div class=\"card h-100\">");
                    sb.Append("<img src=\"images/" + row["productimage"] + "\" class=\"card-img-top\" alt=\"Product\">");
                    sb.Append(" <div class=\"card-body\">");
                    sb.Append("<h5 class=\"card-title\"><a href=ProductDetails.aspx?productID=" + row["productid"] + ">" + row["productname"] + "</a></h5>");
                    sb.Append("<p class=\"card-text\">$" + row["productprice"] + "</p>");
                    sb.Append(" <a class=\"btn btn-warning\" href=\"AddToCart.aspx?productID=" + row["productid"] + "\" >Add to Cart </a>");
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append(" </div>");

                    productdiv.InnerHtml = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error Occured During Product List Page Load::", ex);
            }
        }

        private void Page_Init(object sender, EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            
        }
    }
}