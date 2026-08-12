using System.Linq;
using System.Web.Http;
using WebActivatorEx;
using devShop;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using System.Web.Http.Description;
using System.IO;
using System;
using System.Collections.Generic;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace devShop
{
    public class SwaggerConfig
    {
        public static void Register()
        {
var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
       .EnableSwagger(c =>
  {
        // API version and metadata
         c.SingleApiVersion("v1", "devShop Product Reviews API")
      .Description("REST API for managing product reviews and ratings")
         .Contact(cc => cc
              .Name("devShop API Team")
   .Email("api@devshop.com"))
          .License(lc => lc
          .Name("MIT License")
  .Url("https://opensource.org/licenses/MIT"));

              // Enable XML documentation comments
             var xmlFile = $"{thisAssembly.GetName().Name}.xml";
       var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", xmlFile);
      if (File.Exists(xmlPath))
   {
       c.IncludeXmlComments(xmlPath);
             }

          // Set the base path/server URL - Azure compatible
         c.RootUrl(req =>
        {
      // Check for Azure-specific environment variable
      var websiteHostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
     
       if (!string.IsNullOrEmpty(websiteHostname))
    {
          // Running on Azure - use HTTPS and hostname from environment
          return $"https://{websiteHostname}";
           }
     else
    {
      // Running locally - use request URL
     var scheme = req.RequestUri.Scheme;
      var host = req.RequestUri.Host;
 var port = req.RequestUri.Port;
  
      // Include port if not default
     var portPart = (scheme == "https" && port == 443) || (scheme == "http" && port == 80)
         ? ""
      : $":{port}";
          
       return $"{scheme}://{host}{portPart}";
          }
    });

   // Describe all enums as strings for better readability
     c.DescribeAllEnumsAsStrings();

            c.ApiKey("X-Api-Key")
                .Description("API key required for write operations.")
                .Name("X-Api-Key")
                .In("header");
            c.OperationFilter<ApiKeyOperationFilter>();

     // Resolve conflicting actions (same route, different methods)
             c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

     // Use fully qualified names for schema IDs to avoid conflicts
   c.SchemaId(type => type.FriendlyId(true));

  // Ignore obsolete actions
         c.IgnoreObsoleteActions();

 // Add custom response messages
                  c.OperationFilter<AddResponseHeadersFilter>();
        
                // Remove or comment out the following line, as 'EnableAnnotations' is not available on SwaggerDocsConfig:
                // c.EnableAnnotations();
 })
             .EnableSwaggerUi(c =>
         {
 // Customize Swagger UI
            c.DocumentTitle("devShop Product Reviews API Documentation");
      
            // Set default expansion
c.DocExpansion(DocExpansion.List);
        
     // Write operations require the configured API key.
     c.EnableApiKeySupport("X-Api-Key", "header");

    // Disable validator badge
        c.DisableValidator();
      
           // Enable "Try it out" for all operations
    c.SupportedSubmitMethods("GET", "POST", "PUT", "DELETE", "PATCH");
     });
        }
    }

    public class ApiKeyOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var requiresApiKey =
                apiDescription.ActionDescriptor.GetCustomAttributes<ApiKeyAuthorizeAttribute>().Any() ||
                apiDescription.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<ApiKeyAuthorizeAttribute>().Any();

            if (!requiresApiKey)
                return;

            if (operation.security == null)
                operation.security = new List<IDictionary<string, IEnumerable<string>>>();

            operation.security.Add(new Dictionary<string, IEnumerable<string>>
            {
                { "X-Api-Key", Array.Empty<string>() }
            });

            if (!operation.responses.ContainsKey("401"))
                operation.responses.Add("401", new Response { description = "API key is missing or invalid." });
        }
    }

    /// <summary>
    /// Custom operation filter to add response headers and Azure-specific metadata
    /// </summary>
    public class AddResponseHeadersFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
       if (operation.responses == null)
              operation.responses = new Dictionary<string, Response>();

            // Add common response headers
  foreach (var response in operation.responses.Values)
            {
        if (response.headers == null)
              response.headers = new Dictionary<string, Header>();

         if (!response.headers.ContainsKey("X-Request-ID"))
  {
 response.headers["X-Request-ID"] = new Header
     {
         description = "Unique request identifier for tracking",
           type = "string"
    };
            }
            }

       // Add server information for Azure AI Foundry compatibility
            var websiteHostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
 if (!string.IsNullOrEmpty(websiteHostname))
      {
      // Add vendor extension for Azure deployment
          if (operation.vendorExtensions == null)
   operation.vendorExtensions = new Dictionary<string, object>();
   
     operation.vendorExtensions["x-azure-deployment"] = new
    {
          hostname = websiteHostname,
     platform = "Azure App Service"
         };
 }
        }
    }
}
