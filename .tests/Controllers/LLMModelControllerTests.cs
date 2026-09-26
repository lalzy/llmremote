// LLMModelControllerTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Bogus;
using AutoBogus;
using LLMRemote.Tests.Util;
using LLMRemote.Models;
using System.Net.Http.Json;

namespace LLMRemote.Tests;

public class LLMModelControllerTests : IClassFixture<WebApplicationFactory<Program>>{
    private readonly Faker _faker = new();
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const string URL = "/api/llmmodel";

    public LLMModelControllerTests(WebApplicationFactory<Program> factory){
        (_factory, _client) = ControllersUtil.Setup(factory);
    }

    // Helpers
    private LLMModelRequest CreateRequest(){
        return new AutoFaker<LLMModelRequest>()
            .RuleFor(r => r.Context, f => f.Random.Int(1, int.MaxValue))
            .Generate();
    }

    [Fact]
    public async Task Add_Ok(){
        var response = await _client.PostAsJsonAsync($"{URL}/add", CreateRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public static TheoryData<string, Action<LLMModelRequest>> InvalidRequest => new(){
       { "Name null",      r => r.Name = null! },
        { "Name empty",     r => r.Name = "" },
        { "FilePath null",  r => r.FilePath = null! },
        { "FilePath empty", r => r.FilePath = "" },
        { "Context zero",   r => r.Context = 0 },
        { "Context -1",     r => r.Context = -1 },
    };


    [Theory]
    [MemberData(nameof(InvalidRequest))]
    public async Task Add_InvalidRequest_BadRequest(string reason, Action<LLMModelRequest> breakIt){
        var request = CreateRequest();
        breakIt(request);

        var response = await _client.PostAsJsonAsync($"{URL}/add", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
