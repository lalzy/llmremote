using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using Bogus;

namespace LLMRemote.Tests.Services;

public class LlamaServiceTests : DatabaseTestBase{
    private readonly LlamaService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new Faker();
    
    public LlamaServiceTests(DatabaseFixture fixture) : base(fixture){
        var apps = new OptionsWrapper<Apps>(new Apps {
            Llama = new AppConfig { Path = _faker.System.FilePath(), Port = _faker.Internet.Port() }
        });
        
        _service = new LlamaService(fixture.CreateContext(), _process, apps);
    }

    [Fact]
    public void StartServer_RunsLLamaServer(){
        _service.StartServer(_faker.Random.Guid());

        Assert.Equal(1, _process.StartCount);
        Assert.False(_process.HasExited);
    }
    
    [Fact]
    public void StartServer_RunsOnlyOneProcess(){
        Guid ID = _faker.Random.Guid();
        
        _service.StartServer(ID);
        Assert.Equal(0, _process.StopCount);
        _service.StartServer(ID);
        Assert.Equal(1, _process.StopCount);
    }

    [Fact]
    public void StopServer_StopsLLamaServer(){
        var filePath = _faker.System.FilePath();
        _service.StartServer(_faker.Random.Guid());
        
        _service.StopServer();
        Assert.True(_process.HasExited);
    }

}
