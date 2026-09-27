// HangingHttpHandler.cs
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LLMRemote.Tests.Util;

public class HangingHttpHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return new HttpResponseMessage();
    }
}
