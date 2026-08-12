using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace devShop
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ApiKeyAuthorizeAttribute : AuthorizeAttribute
    {
        private const string HeaderName = "X-Api-Key";

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var configuredKey = ConfigurationManager.AppSettings["ApiKey"];
            if (string.IsNullOrWhiteSpace(configuredKey))
                return false;

            if (!actionContext.Request.Headers.TryGetValues(HeaderName, out var values))
                return false;

            var suppliedKeys = values.ToArray();
            return suppliedKeys.Length == 1 && FixedTimeEquals(configuredKey, suppliedKeys[0]);
        }

        private static bool FixedTimeEquals(string expected, string supplied)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
            var difference = expectedBytes.Length ^ suppliedBytes.Length;
            var length = Math.Max(expectedBytes.Length, suppliedBytes.Length);

            for (var i = 0; i < length; i++)
            {
                var expectedByte = i < expectedBytes.Length ? expectedBytes[i] : (byte)0;
                var suppliedByte = i < suppliedBytes.Length ? suppliedBytes[i] : (byte)0;
                difference |= expectedByte ^ suppliedByte;
            }

            return difference == 0;
        }
    }
}
