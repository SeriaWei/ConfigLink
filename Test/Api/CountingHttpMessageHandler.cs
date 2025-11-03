using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigLink.Api.Tests
{
    internal class CountingHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; set; } = new HttpResponseMessage();
        public int MaxFailures { get; set; } = 0;
        public int CallCount { get; private set; } = 0;
        public HttpRequestMessage Request { get; private set; } = null!;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            Request = request;
            
            // Return failure response for first MaxFailures calls, then success
            if (CallCount <= MaxFailures)
            {
                return await Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    Content = new StringContent("{\"error\":\"temporarily_unavailable\"}", System.Text.Encoding.UTF8, "application/json")
                });
            }
            else
            {
                if (Response != null)
                {
                    var originalContent = await Response.Content.ReadAsStringAsync();
                    return new HttpResponseMessage(Response.StatusCode)
                    {
                        Content = new StringContent(originalContent, System.Text.Encoding.UTF8, "application/json")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }
    }
}