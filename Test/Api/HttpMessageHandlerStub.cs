using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ConfigLink.Api.Tests
{
    internal class HttpMessageHandlerStub : HttpMessageHandler
    {
        public HttpResponseMessage? Response { get; set; }
        public HttpRequestMessage? Request { get; private set; }
        public int CallCount { get; private set; } = 0;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            
            if (Response != null)
            {
                var originalContent = await Response.Content.ReadAsStringAsync();
                var clonedResponse = new HttpResponseMessage(Response.StatusCode)
                {
                    Content = new StringContent(originalContent, Encoding.UTF8, "application/json")
                };
                
                foreach (var header in Response.Headers)
                {
                    clonedResponse.Headers.Add(header.Key, header.Value);
                }
                
                return clonedResponse;
            }
            
            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        }
        
        public Uri? RequestUri => Request?.RequestUri;
    }
}