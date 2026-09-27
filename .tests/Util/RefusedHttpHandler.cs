// RefusedHttpHandler.cs

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LLMRemote.Tests.Util;

public class RefusedHttpHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => throw new HttpRequestException("Connection refused");
}
