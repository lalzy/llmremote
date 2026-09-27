// LlamaServiceTests.cs

using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Tests.Factories;
using Bogus;
using LLMRemote.Models;
using System.Net;

namespace LLMRemote.Tests.Services;

public class LlamaServiceTests : DatabaseTestBase{
    private readonly LlamaService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new Faker();
    private readonly OptionsWrapper<Apps> _apps;
    
    public LlamaServiceTests(DatabaseFixture fixture) : base (fixture){
        _apps = new OptionsWrapper<Apps>(new Apps {
                Llama = new AppConfig { Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings ="" }
        });

        _service = CreateService(HttpStatusCode.OK);
    }

    private LlamaService CreateService(HttpStatusCode code) => new LlamaService(_process, _apps, new HttpClient(new FakeHttpHandler(code)));

    [Fact]
    public void StartServer_RunsLLamaServer(){
        var model = LLMModelFactory.Create(_fixture);
        _service.StartServer(model);

        Assert.Equal(1, _process.StartCount);
        Assert.False(_process.NotRunning);
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
        Assert.True(_process.NotRunning);
    }

    [Fact]
    public async Task RunningP_ReturnOfflineWhenNotStarted(){
        var result = await _service.RunningP();
        Assert.Equal(ServerState.Offline, result);
    }

    [Fact]
    public async Task RunningP_ReturnsOnlineWhenLlamaReturnOK(){
        var service = CreateService(HttpStatusCode.OK);
        service.StartServer(LLMModelFactory.Create(_fixture));

        Assert.Equal(ServerState.Online, (await service.RunningP()));
    }

    [Fact]
    public async Task RunningP_ReturnsLoadingWhenHealthNotOK(){
        var service = CreateService(HttpStatusCode.ServiceUnavailable);
        service.StartServer(LLMModelFactory.Create(_fixture));

        Assert.Equal(ServerState.Loading, (await service.RunningP()));
    }

    [Fact]
    public async Task RunningP_ReturnsOfflineAfterStop(){
        var service = CreateService(HttpStatusCode.OK);
        service.StartServer(LLMModelFactory.Create(_fixture));
        service.StopServer();

        Assert.Equal(ServerState.Offline, (await service.RunningP()));
    }

    [Theory]
    [InlineData(HostOS.Windows, "cmd.exe", "/k \"\"/llama\" -m x\"")]
    [InlineData(HostOS.Linux,   "bash",    "-c \"'/llama' -m x; exec bash\"")]
    [InlineData(HostOS.Mac,   "zsh",     "-c \"'/llama' -m x; exec zsh\"")]
    public void CreateOSTerminalCommand_ReturnsCorrectCommandPerOS(HostOS os, string expectedFile, string expectedArgs){
        var (file, args) = LlamaService.CreateOSTerminalCommand(os, "/llama", "-m x");

        Assert.Equal(expectedFile, file);
        Assert.Equal(expectedArgs, args);
    }
}
