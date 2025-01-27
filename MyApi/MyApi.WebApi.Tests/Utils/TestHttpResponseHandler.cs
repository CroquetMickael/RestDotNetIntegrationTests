
namespace MyApi.WebApi.Tests.Utils
{
    public class TestHttpResponseHandler : DelegatingHandler
    {

        public TestHttpResponseHandler()
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
