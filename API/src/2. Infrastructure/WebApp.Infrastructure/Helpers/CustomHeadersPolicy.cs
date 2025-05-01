using Azure.Core.Pipeline;
using Azure.Core;

namespace WebApp.Infrastructure.Helpers
{
    public class CustomHeadersPolicy : HttpPipelineSynchronousPolicy
    {
        public override void OnSendingRequest(HttpMessage message)
        {
            message.Request.Headers.Add("x-ms-useragent", "azsdk-net-AIServiceAgents/1.0.0");
            base.OnSendingRequest(message);
        }
    }
}
