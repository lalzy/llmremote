// ComfyControllerTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Bogus;
using System.Net.Http.Json;
using LLMRemote.Tests.Util;

namespace LLMRemote.Tests;

public class ComfyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Faker _faker = new();
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ComfyControllerTests(WebApplicationFactory<Program> factory){
        (_factory, _client) = ControllersUtil.Setup(factory);
    }

    [Fact]
    public async Task StartServer_Ok(){
        var model = ControllersUtil.CreateLLMModel(_factory);
        var response = await _client.PostAsync($"/api/comfy/start", null);
    var body = await response.Content.ReadAsStringAsync();
Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StopServer_Ok(){
        var response = await _client.PostAsync("/api/comfy/stop/", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ServerHealth_Ok(){
        var response = await _client.GetAsync("/api/comfy/health/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
