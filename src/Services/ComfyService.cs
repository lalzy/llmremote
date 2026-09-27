// ComfyService.cs// LLamaService.cs

using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using LLMRemote.Options;
using LLMRemote.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace LLMRemote.Services;

public class ComfyService([FromKeyedServices("comfy")] IManagedProcess process, IOptions<Apps> apps, HttpClient client){
    private readonly IManagedProcess _process = process;
    private readonly AppConfig _comfyConfig = apps.Value.ComfyUI;
    private readonly HttpClient _client = client;

    private string CreateComfyUIArgument() { return ""; }

    /// <summary>Starts the ComfyUI-server process</summary>
    /// <remarks>Opens an external terminal that then runs comfyUI to circumvent it closing on errors </remarks>
    public void StartServer(){
        if(!_process.NotRunning) return;
        _process.Start(_comfyConfig.Path, CreateComfyUIArgument());
    }

    /// <summary>Stops the ComfyUI-Server process </summary>
    public void StopServer(){
        if(_process.NotRunning) return;
        
        _process.Stop();
    }

    /// <summary>Get current running state of ComfyUI</summary>
    /// <returns>
    /// <see cref="ServerState.Online"/> If the server is ready
    /// <see cref="ServerState.Loading"/> If the server is booting up
    /// <see cref="ServerState.Offline"/> If the server is not running
    ///</returns>
    public async Task<ServerState> RunningP() {
        if(_process.NotRunning) return ServerState.Offline;
        
        var response = await _client.GetAsync($"http://localHost:{_comfyConfig.Port}");

        if(response.StatusCode == HttpStatusCode.OK) return ServerState.Online;
        else return ServerState.Loading;
    }
}
