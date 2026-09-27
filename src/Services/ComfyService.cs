// ComfyService.cs// LLamaService.cs

using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using LLMRemote.Options;
using LLMRemote.Models;
using System.Net.Http;
using System.Threading.Tasks;
using LLMRemote.Util;

namespace LLMRemote.Services;

public class ComfyService([FromKeyedServices("comfy")] IManagedProcess process, IOptions<Apps> apps, HttpClient client, HostOS os = OpenTerminalHelper.CurrentOS){
    private readonly IManagedProcess _process = process;
    private readonly AppConfig _comfyConfig = apps.Value.ComfyUI;
    private readonly HttpClient _client = client;
    private readonly HostOS CurrentOS = os;

    private string CreateComfyUIArgument() {
        string mainPy = Path.Combine(_comfyConfig.Path, "ComfyUI", "main.py");
        string windowsFlag = CurrentOS == HostOS.Windows ? "--windows-standalone-build " : "";

        return $"-s \"{mainPy}\" {windowsFlag}--port {_comfyConfig.Port} {_comfyConfig.OtherSettings}";
    }

    public void StartServer(){
        if(!_process.NotRunning) return;

        string python = Path.Combine(_comfyConfig.Path, "python_embeded", "python.exe");

        (string file, string arguments) = OpenTerminalHelper.CreateOSTerminalCommand(CurrentOS, python, CreateComfyUIArgument());

        _process.Start(file, arguments);
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

        try{
            using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(1));
            var response = await _client.GetAsync($"http://127.0.0.1:{_comfyConfig.Port}", timeout.Token);

            if(response.StatusCode == HttpStatusCode.OK) return ServerState.Online;
            return ServerState.Loading;
        }
        catch(HttpRequestException){
            return ServerState.Loading;
        }
        catch(TaskCanceledException){
            return ServerState.Loading;
        }
    }
}
