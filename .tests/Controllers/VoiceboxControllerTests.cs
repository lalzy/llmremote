// VoiceboxControllerTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Bogus;
using System.Net.Http.Json;
using LLMRemote.Tests.Util;
using Microsoft.Extensions.DependencyInjection;
using LLMRemote.Services;
using LLMRemote.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMRemote.Tests.Controllers;

public class VoiceboxControllerTests : IClassFixture<WebApplicationFactory<Program>>{
    private readonly Faker _faker = new();
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public VoiceboxControllerTests(WebApplicationFactory<Program> factory){
        (_factory, _client) = ControllersUtil.Setup(factory);
    }

    [Fact]
    public async Task StartServer_Ok(){
        var response = await _client.PostAsync($"/api/voicebox/start", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StartServer_Starts(){
        await _client.PostAsync("/api/voicebox/start", null);

        var process = (FakeProcess)_factory.Services.GetRequiredKeyedService<IManagedProcess>("voicebox");
        Assert.Equal(1, process.StartCount);
    }

    [Fact]
    public async Task StopServer_Ok(){
        var response = await _client.PostAsync("api/voicebox/stop", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StopServer_Stops(){
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<VoiceboxService>().StartServer();

        var process = (FakeProcess)_factory.Services.GetRequiredKeyedService<IManagedProcess>("voicebox");
        Assert.False(process.NotRunning);

        await _client.PostAsync("api/voicebox/stop", null);
        Assert.True(process.NotRunning);
    }

    [Fact]
    public async Task ServerHealth_Ok(){
        var response = await _client.GetAsync("api/voicebox/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsHealth(){
        var response = await _client.GetAsync("api/voicebox/health");
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var data = await response.Content.ReadFromJsonAsync<ServerState>(options);
        Assert.Equal(ServerState.Offline, data);
    }
}
