// LLamaControllerTests.csdef

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LLMRemote.Tests.Util;

namespace LLMRemote.Tests;

public class LlamaControllerTests : IClassFixture<WebApplicationFactory<Program>>{
    private readonly Faker _faker = new();
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public LlamaControllerTests(WebApplicationFactory<Program> factory){
        (_factory, _client) = Controllers.Setup(factory);
    }

    [Fact]
    public async Task StartServer_Ok(){
        var response = await _client.GetAsync("/api/llama/start");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StopServer_Ok(){
        var response = await _client.GetAsync("/api/llama/stop");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
