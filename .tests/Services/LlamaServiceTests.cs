using LLMRemote.Services;
using LLMRemote.Tests.Util;
using Bogus;

namespace LLMRemote.Tests.Services;

public class LlamaServiceTests : DatabaseTestBase{
    private readonly LlamaService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new Faker();

    public LlamaServiceTests(DatabaseFixture fixture) : base(fixture){
        _service = new LlamaService(fixture.CreateContext(), _process);
    }

    [Fact]
    public void StartServer_RunsLLamaServer(){
        _service.StartServer(_faker.System.FilePath(), _faker.Random.Guid());

        Assert.Equal(1, _process.StartCount);
        Assert.False(_process.HasExited);
    }
    
    [Fact]
    public void StartServer_RunsOnlyOneProcess(){
        Guid ID = _faker.Random.Guid();
        string path = _faker.System.FilePath();
        _service.StartServer(path, ID);
        Assert.Equal(0, _process.StopCount);
        _service.StartServer(path, ID);
        Assert.Equal(1, _process.StopCount);
    }

    [Fact]
    public void StopServer_StopsLLamaServer(){
        var filePath = _faker.System.FilePath();
        _service.StartServer(filePath, _faker.Random.Guid());
        
        _service.StopServer();
        Assert.True(_process.HasExited);
    }

}
