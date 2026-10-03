// VoiceboxServiceTests.cs

using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Tests.Factories;
using Bogus;
using LLMRemote.Models;
using System.Net;
using LLMRemote.Util;

namespace LLMRemote.Tests.Services;

public class VoiceboxServiceTests{
    private readonly VoiceboxService _service;
    private readonly Faker _faker = new();
    private readonly FakeProcess _process = new();
    private readonly OptionsWrapper<Apps> _apps;
    
    public VoiceboxServiceTests(){
        _apps = new OptionsWrapper<Apps>(new Apps{
                Voicebox = new AppConfig {Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings = ""}
        });

        _service = CreateService(HttpStatusCode.OK); 
    }
    private VoiceboxService CreateService(HttpStatusCode code) => new VoiceboxService(_process, _apps, new HttpClient(new FakeHttpHandler(code)));

    [Fact]
    public void StartServer_RunsVoicebox(){
        _service.StartServer();

        Assert.Equal(1, _process.StartCount);
        Assert.False(_process.NotRunning);
    }

    [Fact]
    public void StartServer_RunsOnlyOneProcess(){
        _service.StartServer();
        Assert.Equal(0, _process.StopCount);
        _service.StartServer();
        Assert.Equal(1, _process.StopCount);
    }

    [Fact]
    public void StopServer_StopsVoicebox(){
        _service.StartServer();
        Assert.True(!_process.NotRunning);
        _service.StopServer();
        Assert.True(_process.NotRunning);
    }

    [Fact]
    public async Task RunningP_ReturnOfflineWhenNotStarted(){
        var result = await _service.RunningP();
        Assert.Equal(ServerState.Offline, result);
    }

    [Fact]
    public async Task RunningP_ReturnsOnlineWhenVoiceboxReturnOK(){
        var service = CreateService(HttpStatusCode.OK);
        service.StartServer();
        Assert.Equal(ServerState.Online, (await service.RunningP()));
    }

    [Fact]
    public async Task RunningP_ReturnsLoadingWhenHealthNotOK(){
        var service = CreateService(HttpStatusCode.ServiceUnavailable);
        service.StartServer();

        Assert.Equal(ServerState.Loading, (await service.RunningP()));
    }

    [Fact]
    public async Task RunningP_ReturnsOfflineAfterStop(){
        var service = CreateService(HttpStatusCode.OK);
        service.StartServer();
        service.StopServer();

        Assert.Equal(ServerState.Offline, (await service.RunningP()));
    }
}
