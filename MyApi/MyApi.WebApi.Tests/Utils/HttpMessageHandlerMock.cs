using System.Net;

namespace MyApi.WebApi.Tests.Utils
{

    public class HttpMessageHandlerMock : HttpMessageHandler
    {
        private HttpResponseMessage? _httpResponseMessage;
        private int _errorCounter;

        public void SetResponse(HttpResponseMessage httpResponseMessage)
        {
            _httpResponseMessage = httpResponseMessage;
        }

        public void SetFailedAttemptsAndResponse(HttpResponseMessage httpResponseMessage, int nbError)
        {
            _httpResponseMessage = httpResponseMessage;
            _errorCounter = nbError;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_errorCounter > 0)
            {
                _errorCounter--;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway));
            }

            if (_httpResponseMessage == null)
            {
                throw new InvalidOperationException("Error in request");
            }

            return Task.FromResult(_httpResponseMessage);
        }
    }

}
