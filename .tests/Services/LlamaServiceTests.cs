// LlamaServiceTests.cs

using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Tests.Factories;
using Bogus;

namespace LLMRemote.Tests.Services;

public class LlamaServiceTests : DatabaseTestBase{
    private readonly LlamaService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new Faker();
    
    public LlamaServiceTests(DatabaseFixture fixture) : base (fixture){
        var apps = new OptionsWrapper<Apps>(new Apps {
                Llama = new LlamaConfig { Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings ="" }
        });

        _service = new LlamaService(_process, apps);
    }

    [Fact]
    public void StartServer_RunsLLamaServer(){
        var model = LLMModelFactory.Create(_fixture);
        _service.StartServer(model);

        Assert.Equal(1, _process.StartCount);
        Assert.False(_process.HasExited);
    }
    
    [Fact]
    public void StartServer_RunsOnlyOneProcess(){
        var model = LLMModelFactory.Create(_fixture);
        _service.StartServer(model);
        Assert.Equal(0, _process.StopCount);
        _service.StartServer(model);
        Assert.Equal(1, _process.StopCount);
    }

    [Fact]
    public void StopServer_StopsLLamaServer(){
        var model = LLMModelFactory.Create(_fixture);
        _service.StartServer(model);
        
        _service.StopServer();
        Assert.True(_process.HasExited);
    }

}
