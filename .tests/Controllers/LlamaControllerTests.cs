// LLamaControllerTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Bogus;
using System.Net.Http.Json;
using LLMRemote.Tests.Util;

namespace LLMRemote.Tests;

public class LlamaControllerTests : IClassFixture<WebApplicationFactory<Program>>{
    private readonly Faker _faker = new();
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public LlamaControllerTests(WebApplicationFactory<Program> factory){
        (_factory, _client) = ControllersUtil.Setup(factory);
    }

    [Fact]
    public async Task StartServer_Ok(){
        var model = ControllersUtil.CreateLLMModel(_factory);
        var response = await _client.PostAsJsonAsync($"/api/llama/start", model); 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StopServer_Ok(){
        var response = await _client.PostAsync("/api/llama/stop/", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
