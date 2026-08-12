using System;

namespace devShop
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                errorLBL.ForeColor = System.Drawing.Color.Red;
                errorLBL.Text = "Something went wrong while processing your request. Please try again later.";
            }
        }
    }
}