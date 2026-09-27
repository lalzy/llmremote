// LLamaService.cs

using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using LLMRemote.Options;
using LLMRemote.Models;
using LLMRemote.Util;
using System.Net.Http;
using System.Threading.Tasks;

namespace LLMRemote.Services;

///<summary>Service to manage llama-server process</summary>
public class LlamaService([FromKeyedServices("llama")] IManagedProcess process, IOptions<Apps> apps, HttpClient client){
    private readonly IManagedProcess _process = process;
    private readonly AppConfig _llamaConfig = apps.Value.Llama;
    private readonly HttpClient _client = client;


    private string CreateLlamaArgument(LLMModel model){
        var arguments = new StringBuilder();
        arguments.Append($"-m \"{model.FilePath}\" ");
        arguments.Append($"--ctx-size {model.Context} ");
        arguments.Append($"--port {_llamaConfig.Port} ");
        arguments.Append(_llamaConfig.OtherSettings);
        return arguments.ToString();
    }


    /// <summary>Starts the Llama-server process</summary>
    /// <param name="model">The LLM Model object returned from the database / llmmodel endpoint</param>
    /// <remarks>Opens an external terminal that then runs Llama-server to circumvent it closing on errors </remarks>
    public void StartServer(LLMModel model){
        
        // Only one server instance should run at a time
        if(!_process.NotRunning) _process.Stop();

        var (file, args) = OpenTerminalHelper.CreateOSTerminalCommand(OpenTerminalHelper.CurrentOS, _llamaConfig.Path, CreateLlamaArgument(model));
        _process.Start(file, args);
    }

    /// <summary>Stop Llama Server process</summary>
    public void StopServer(){
        _process.Stop();
    }

    /// <summary>Get current running state of Llama</summary>
    /// <returns>
    /// <see cref="ServerState.Online"/> If the server is ready
    /// <see cref="ServerState.Loading"/> If the server is booting up
    /// <see cref="ServerState.Offline"/> If the server is not running
    ///</returns>
    public async Task<ServerState> RunningP(){
        if(!_process.NotRunning){
            var response = await _client.GetAsync($"http://localhost:{_llamaConfig.Port}/health");

            if(response.StatusCode == HttpStatusCode.OK){
                return ServerState.Online;
            }else{
                return ServerState.Loading;
            }
        }
        return ServerState.Offline;
        
    }
}
