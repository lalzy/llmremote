// LLMModelControllerTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Bogus;
using AutoBogus;
using LLMRemote.Tests.Util;
using LLMRemote.Models;
using System.Net.Http.Json;
using LLMRemote.Services;

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

    [Fact]
    public async Task Get_Ok(){
        var model = ControllersUtil.CreateLLMModel(_factory);

        var response = await _client.GetAsync($"{URL}/{model.ID}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Ok(){
        var response = await _client.GetAsync($"{URL}/all");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private T WithService<T>(Func<LLMModelsService, T> action){
        using var scope = _factory.Services.CreateScope();
        return action(scope.ServiceProvider.GetRequiredService<LLMModelsService>());
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 5)]
    [InlineData(1, 10)]
    public async Task GetAll_PageRequest(int page, int count){
        Enumerable.Range(0, count*2).Select(_ => ControllersUtil.CreateLLMModel(_factory)).ToList();
        
        var expected = WithService(s => s.GetAll(page: page, count: count));
        var actual = await _client.GetFromJsonAsync<List<LLMModel>>($"{URL}/all?page={page}&count={count}");
        
        Assert.Equal(expected.Select(m => m.ID), actual!.Select(m => m.ID));
    }


    [Theory]
    [InlineData(LLMModelsService.OrderBy.ID)]
    [InlineData(LLMModelsService.OrderBy.Name)]
    [InlineData(LLMModelsService.OrderBy.Created)]
    [InlineData(LLMModelsService.OrderBy.Updated)]
    public async Task GetAll_SortRequest(LLMModelsService.OrderBy orderBy){
        const int TOTAL = 10;
        Enumerable.Range(0, TOTAL).Select(_ => ControllersUtil.CreateLLMModel(_factory)).ToList();

        var expected = WithService(s => s.GetAll(orderBy: orderBy));
        var actual = await _client.GetFromJsonAsync<List<LLMModel>>($"{URL}/all?orderBy={orderBy}");
        
        Assert.Equal(expected.Select(m => m.ID), actual!.Select(m => m.ID));
    }

    [Theory]
    [InlineData(0, 5, LLMModelsService.OrderBy.ID)]
    [InlineData(0, 15, LLMModelsService.OrderBy.ID)]
    [InlineData(1, 0, LLMModelsService.OrderBy.ID)]
    [InlineData(5, -1, LLMModelsService.OrderBy.ID)]
    [InlineData(1, 5, (LLMModelsService.OrderBy)99)]
    public async Task GetAll_InvalidParams_BadRequest(int page, int count, LLMModelsService.OrderBy orderBy){
        var response = await _client.GetAsync($"{URL}/all?page={page}&count={count}&orderBy={orderBy}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
