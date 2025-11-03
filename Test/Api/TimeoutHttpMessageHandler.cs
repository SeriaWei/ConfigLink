using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigLink.Api.Tests
{
    internal class TimeoutHttpMessageHandler : HttpMessageHandler
    {
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(5); // Default 5 second delay
        public HttpResponseMessage Response { get; set; } = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Simulate a long-running request by delaying
            var delayTask = Task.Delay(Delay, cancellationToken);
            
            // If cancellation token is triggered (e.g. due to timeout), the delay will be cancelled
            await delayTask;
            
            // If we get here, return the response
            var originalContent = await Response.Content.ReadAsStringAsync();
            return new HttpResponseMessage(Response.StatusCode)
            {
                Content = new StringContent(originalContent, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}