// NvidiaMonitorControllerTests.cs

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Bogus;
using System.Net.Http.Json;
using LLMRemote.Tests.Util;

public class NvidiaMonitorControllerTests : IClassFixture<WebApplicationFactory<Program>>{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new();

    public NvidiaMonitorControllerTests(WebApplicationFactory<Program> factory){
        _factory = factory.WithWebHostBuilder(b => b.ConfigureServices(s => {
            s.RemoveAllKeyed<IManagedProcess>("nvidia");
            s.AddKeyedSingleton<IManagedProcess>("nvidia", _process);
        }));
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetNvidiaUsage_Ok(){
        var response = await _client.GetAsync("/api/nvidia/usage");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetNvidiaUsage_VerifyJson(){
        var name = _faker.Commerce.ProductName();
        var gpu = _faker.Random.Int(0, 100).ToString();
        var memoryTotal = _faker.PickRandom(new[] { 2048, 4096, 8192, 12288, 16384});
        var memory = _faker.Random.Int(0, memoryTotal).ToString();

        _factory.Services.GetRequiredService<NvidiaMonitorService>();
        _process.Output!($"{name}, {gpu}, {memory}, {memoryTotal}");

        var result = await _client.GetFromJsonAsync<Dictionary<string, Dictionary<string, string>>>("/api/nvidia/usage");

        Assert.Equal(gpu, result![name]["gpu"]);
        Assert.Equal(memory, result[name]["memory"]);
    }
}
