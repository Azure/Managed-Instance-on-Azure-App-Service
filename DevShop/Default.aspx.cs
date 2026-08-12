using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Web.UI;
using log4net;

namespace devShop
{

    public partial class CDefault : System.Web.UI.Page
    {


        public int ProductID;

        private static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(CDefault));
        public CDefault()
        {
            Page.Init += new System.EventHandler(Page_Init);
        }


        private void Page_Load(object sender, System.EventArgs e)
        {
           
            try
            {
               Log.Debug("Page_Load: Loading most popular products");
              
                // Obtain list of favorite items
                devShop.ProductsDB products = new devShop.ProductsDB();

            // display the list of favorite product items
            DataTable popProds = products.GetMostPopularProductsOfWeek();

            // Fix: Use DataTableReader instead of SqlDataReader
            DataTableReader popProdsSQLReader = popProds.CreateDataReader(); // Create the data reader
            popProds.Load(popProdsSQLReader);
            StringBuilder sb = new StringBuilder();
            while (popProdsSQLReader.Read())
            {
                sb.Append("<div class=\"col\">");
                sb.Append("<div class=\"card h-100\">");
                sb.Append("<img src=\"images/" + popProdsSQLReader["productimage"] + "\" class=\"card-img-top\" alt=\"Product\">");
                sb.Append(" <div class=\"card-body\">");
                sb.Append("<h5 class=\"card-title\"><a href=\"productdetails.aspx?ProductID=" + popProdsSQLReader["productid"] + "\"</a>" + popProdsSQLReader["productname"] + "</h5>");
                sb.Append("<p class=\"card-text\">$" + popProdsSQLReader["productprice"] + "</p>");
                sb.Append(" <a class=\"btn btn-warning\" href=\"#\" >Add to Cart </a>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append(" </div>");
                productdiv.InnerHtml = sb.ToString();

            }
            }
            catch (Exception ex)
            {
                // Log the exception
                Log.Error("Error loading most popular products", ex);
              
            }

        }

        private void Page_Init(object sender, EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
        }

        #region Web Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);

        }
        #endregion

    }
}
