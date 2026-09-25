using LLMRemote.Services;
using LLMRemote.Tests.Util;
using Bogus;

namespace LLMRemote.Tests.Services;

public class LlamaServiceTests : DatabaseTestBase{
    private readonly LlamaService _service;
    private readonly Faker _faker = new Faker();

    public LlamaServiceTests(DatabaseFixture fixture) : base(fixture){
        _service = new LlamaService(fixture.CreateContext());
    }

    [Fact]
    public void Start_RunsLLamaServer(){
        var runner = new FakeProcessRunner();

        _service.StartServer(_faker.Random.Guid());

        Assert.Equal(1, runner.StartCount);
        Assert.Equal("llama-server.exe", runner.FileName);
        Assert.False(runner.LastProcess!.HasExited);
    }
}
