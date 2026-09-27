// ShutdownTimerControllerTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Bogus;
using System.Net.Http.Json;
using LLMRemote.Tests.Util;
using LLMRemote.Services;

namespace LLMRemote.Tests;


public class ShutdownTimerControllerTests : IClassFixture<WebApplicationFactory<Program>>{
    private readonly Faker _faker = new();
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ShutdownTimerControllerTests(WebApplicationFactory<Program> factory){
        (_factory, _client) = ControllersUtil.Setup(factory);
    }

    [Fact]
    public async Task SetTimer_NoContent(){
        var response = await _client.PostAsync($"/api/timer/set?duration=300",null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ShutdownTime_Ok(){
        var service = _factory.Services.GetRequiredService<ShutdownTimerService>();
        service.Set(3600);
        var response = await _client.GetAsync($"/api/timer/shutdowntime");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelTimer_NoContent(){
        var response = await _client.PostAsync($"/api/timer/cancel",null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
