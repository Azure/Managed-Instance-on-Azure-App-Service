using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace devShop
{
    public sealed class ServerErrorLoggingHandler : DelegatingHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerErrorLoggingHandler));

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                Log.ErrorFormat("Request {0} {1} returned HTTP 500.", request.Method, request.RequestUri);

            return response;
        }
    }
}
