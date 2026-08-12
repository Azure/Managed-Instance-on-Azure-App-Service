using System;
using System.Web;
using System.Web.Http;
using log4net;

namespace devShop
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // Swagger is auto-registered via WebActivatorEx PreApplicationStartMethod in SwaggerConfig
        }

        protected void Application_Error(object sender, EventArgs e) 
        { 
            var ex = Server.GetLastError(); 
            System.Diagnostics.Trace.WriteLine("FATAL: " + ex); 
        }
    }
}