// ComfyServiceTests.cs

using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Tests.Factories;
using Bogus;
using LLMRemote.Models;
using System.Net;

namespace LLMRemote.Tests.Services;

public class ComfyServiceTests{
    private readonly ComfyService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new Faker();
    private readonly OptionsWrapper<Apps> _apps;

    public ComfyServiceTests(){
        _apps = new OptionsWrapper<Apps>(new Apps {
                ComfyUI = new AppConfig { Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings ="" }
        });

        _service = CreateService(HttpStatusCode.OK);
    }
    
    private ComfyService CreateService(HttpStatusCode code) => new ComfyService(_process, _apps, new HttpClient(new FakeHttpHandler(code)));

    [Fact]
    public async Task RunnigP_ServerRespondsOfflineWhenProcessNotRunning(){
        ServerState result = await _service.RunningP();

        Assert.Equal(ServerState.Offline, result);
    }

    [Fact]
    public async Task RunningP_ServerRespondsOKWhenOnline(){
        var service = CreateService(HttpStatusCode.OK);
        service.StartServer();
        ServerState result = await service.RunningP();

        Assert.Equal(ServerState.Online, result);
    }

    [Fact]
    public async Task RunningP_ServerRespondsLoadingWhenNotConnected(){
        var service = CreateService(HttpStatusCode.BadGateway);
        service.StartServer();
        ServerState result = await service.RunningP();

        Assert.Equal(ServerState.Loading, result);
    }

    [Fact]
    public void StopServer_StopsProcess(){
        _service.StartServer();
        _service.StopServer();
        Assert.Equal(1, _process.StopCount);
    }

    [Fact]
    public void StopServer_ProcessNotRunning_DoesNotStop(){
        _service.StopServer();
        Assert.Equal(0, _process.StopCount);
    }

    [Fact]
    public void SartServer_AlreadyRunning_DoesNotStartAgain(){
        _service.StartServer();
        _service.StartServer();
        Assert.Equal(1, _process.StartCount);
    }
}
