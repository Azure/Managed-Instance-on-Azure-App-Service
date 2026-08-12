using System;
using System.Data;
using System.Text;
using System.Web;

namespace devShop
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(typeof(Site1));

        protected void Page_Load(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"navbar-nav\">");

            try
            {
                var products = new ProductsDB();
                DataTable categories = products.GetProductCategories();

                foreach (DataRow row in categories.Rows)
                {
                    var categoryId = HttpUtility.UrlEncode(Convert.ToString(row["categoryid"]));
                    var categoryName = HttpUtility.HtmlEncode(Convert.ToString(row["categoryname"]));
                    sb.Append("<a class='nav-item nav-link' href='Productslist.aspx?CategoryID=" + categoryId + "'>" + categoryName + " </a>");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error loading navigation categories.", ex);
                sb.Append("<div class='alert alert-danger' role='alert'>Categories are temporarily unavailable.</div>");
            }

            sb.Append("</div>");
            navbarSupportedContent.InnerHtml = sb.ToString();
        }
    }
}