// VoiceboxService.cs

using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using LLMRemote.Options;
using LLMRemote.Models;
using LLMRemote.Util;
using System.Net.Http;
using System.Threading.Tasks;

namespace LLMRemote.Services;

public class VoiceboxService([FromKeyedServices("voicebox")] IManagedProcess process, IOptions<Apps> apps, HttpClient client){
    private readonly IManagedProcess _process = process;
    private readonly AppConfig _voiceboxConfig = apps.Value.Voicebox;
    private readonly HttpClient _client = client;

    public void StartServer(){
        if(!_process.NotRunning) StopServer();
        _process.Start(_voiceboxConfig.Path, _voiceboxConfig.OtherSettings);
    }

    public void StopServer(){
        if(_process == null || _process.NotRunning) return;
        _process.Stop();
    }
    
    public async Task<ServerState> RunningP(){
        if(_process.NotRunning) return ServerState.Offline;
        
        using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(1));
        var response = await _client.GetAsync($"http://127.0.0.1:{_voiceboxConfig.Port}/health", timeout.Token);

        if(response.StatusCode == HttpStatusCode.OK) return ServerState.Online;
        return ServerState.Loading;
    }
}
