// FakeHttpHandler.cs

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LLMRemote.Tests.Util;

public class FakeHttpHandler(HttpStatusCode code, string? body = null) : HttpMessageHandler{
    public Uri? LastRequestUri { get; private set; }
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken){
        LastRequestUri = request.RequestUri;
        var response = new HttpResponseMessage(code);
        if (body is not null) response.Content = new StringContent(body);
        return Task.FromResult(response);
    }
}
