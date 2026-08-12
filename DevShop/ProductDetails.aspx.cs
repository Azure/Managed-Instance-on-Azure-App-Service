using System;
using System.Web.UI;


namespace devShop
{

    public class ProductDetailsPage : System.Web.UI.Page {

        protected System.Web.UI.WebControls.Label brand;
        protected System.Web.UI.WebControls.Label prodname;
        protected System.Web.UI.WebControls.Label proddesc;
        protected System.Web.UI.WebControls.Image mainimage;
        protected System.Web.UI.WebControls.Label prodPrice;
        protected System.Web.UI.WebControls.Label prodDiscPrice;
        protected System.Web.UI.WebControls.HyperLink atcLink;
        protected System.Web.UI.WebControls.Label stockS;
        protected System.Web.UI.WebControls.Label stockM;
        protected System.Web.UI.WebControls.Label stockL;
        protected System.Web.UI.WebControls.Label stockXL;
        protected System.Web.UI.WebControls.Label stockXXL;

        private static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(ProductDetailsPage));
       

        public ProductDetailsPage() {
            Page.Init += new System.EventHandler(Page_Init);
        }

       

        private void Page_Load(object sender, System.EventArgs e) {

            try{
                   Log.Debug("Page_Load: Loading product details page");
           
            // Obtain ProductID from QueryString
            int ProductID = Int32.Parse(Request.Params["ProductID"]);

            // Obtain Product Details
            devShop.ProductsDB products = new devShop.ProductsDB();
            devShop.ProductDetails myProductDetails = products.GetProductDetails(ProductID);

            // Update Controls with Product Details
             brand.Text= myProductDetails.ProductBrand;
            
            prodname.Text = myProductDetails.ProductName;
            proddesc.Text = myProductDetails.ProductDescription;
            mainimage.ImageUrl = "Images/" + myProductDetails.ProductImage;
            atcLink.NavigateUrl = "order.aspx?productName="+myProductDetails.ProductName+"&catID="+myProductDetails.CatID+"&totPrice="+myProductDetails.ProductPrice+"&prodID="+ProductID+"";
            prodPrice.Text=myProductDetails.ProductPrice.ToString();
            Double discount = (myProductDetails.ProductPrice * 0.9);
            prodDiscPrice.Text=discount.ToString();

            // Populate per-size inventory availability
            try
            {
                var inventory = products.GetProductInventory(ProductID);
                if (inventory != null && inventory.Sizes != null)
                {
                    stockS.Text = FormatStock(inventory.Sizes, "S");
                    stockM.Text = FormatStock(inventory.Sizes, "M");
                    stockL.Text = FormatStock(inventory.Sizes, "L");
                    stockXL.Text = FormatStock(inventory.Sizes, "XL");
                    stockXXL.Text = FormatStock(inventory.Sizes, "XXL");
                }
                else
                {
                    stockS.Text = stockM.Text = stockL.Text = stockXL.Text = stockXXL.Text = "N/A";
                }
            }
            catch (Exception invEx)
            {
                Log.Error("Error loading product inventory for ProductID=" + ProductID, invEx);
            }
            }
            catch (Exception ex)
            {
                Log.Error("Error Occured During Product Details Page Load::", ex);
            }


        }

        private static string FormatStock(System.Collections.Generic.IDictionary<string, int> sizes, string key)
        {
            int qty;
            if (sizes != null && sizes.TryGetValue(key, out qty))
            {
                if (qty <= 0) return "00";
                return qty + String.Empty;
            }
            return "N/A";
        }

        private void Page_Init(object sender, EventArgs e) {
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
        private void InitializeComponent() {    
            this.Load += new System.EventHandler(this.Page_Load);

        }
        #endregion

      
    }
}
