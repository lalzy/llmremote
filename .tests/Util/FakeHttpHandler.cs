// FakeHttpHandler.cs

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LLMRemote.Tests.Util;

public class FakeHttpHandler(HttpStatusCode code) : HttpMessageHandler{
    public Uri? LastRequestUri { get; private set; }
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken){
        LastRequestUri = request.RequestUri;
        return Task.FromResult(new HttpResponseMessage(code));
    }
}
