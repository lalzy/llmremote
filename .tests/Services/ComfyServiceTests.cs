// ComfyServiceTests.cs

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

public class ComfyServiceTests{
    private readonly ComfyService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new Faker();
    private readonly OptionsWrapper<Apps> _apps;

    public ComfyServiceTests(){
        _apps = new OptionsWrapper<Apps>(new Apps {
                ComfyUI = new AppConfig { Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings = _faker.Lorem.Word() }
        });

        _service = CreateService(HttpStatusCode.OK);
    }
    
    private ComfyService CreateService(HttpStatusCode code, HostOS os = HostOS.Windows) =>
        new ComfyService(_process, _apps, new HttpClient(new FakeHttpHandler(code)), os);

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

    [Theory]
    [InlineData(HostOS.Windows, "cmd.exe")]
    [InlineData(HostOS.Linux,   "bash")]
    [InlineData(HostOS.Mac,     "zsh")]
    public void StartServer_UsesTerminalForOS(HostOS os, string expectedFile){
        ComfyService service = CreateService(HttpStatusCode.OK, os);

        service.StartServer();

        Assert.Equal(expectedFile, _process.FileName);
    }
    
    [Theory]
    [InlineData(HostOS.Linux)]
    [InlineData(HostOS.Mac)]
    public void StartServer_NonWindows_NoWindowsFlag(HostOS os){
        ComfyService service = CreateService(HttpStatusCode.OK, os);

        service.StartServer();

        Assert.DoesNotContain("--windows-standalone-build", _process.Arguments);
    }


    [Fact]
    public async Task RunningP_ConnectionRefused_ReturnsLoading(){
        ComfyService service = new ComfyService(_process, _apps, new HttpClient(new RefusedHttpHandler()), HostOS.Windows);
        service.StartServer();

        ServerState result = await service.RunningP();

        Assert.Equal(ServerState.Loading, result);
    }

    [Fact]
    public async Task RunningP_RequestHangs_ReturnsLoading(){
        ComfyService service = new ComfyService(_process, _apps, new HttpClient(new HangingHttpHandler()), HostOS.Windows);
        service.StartServer();

        ServerState result = await service.RunningP();

        Assert.Equal(ServerState.Loading, result);
    }
}
