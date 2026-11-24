// LLamaService.cs

using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using LLMRemote.Options;
using LLMRemote.Models;
using LLMRemote.Util;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LLMRemote.Services;

///<summary>Service to manage llama-server process</summary>
public class LlamaService([FromKeyedServices("llama")] IManagedProcess process, IOptions<Apps> apps, HttpClient client){
    private readonly IManagedProcess _process = process;
    private readonly AppConfig _llamaConfig = apps.Value.Llama;
    private readonly HttpClient _client = client;
    private int _activePort = apps.Value.Llama.Port;

    private int? GetPort(string? arguments){
        if(arguments == null) return null;
        if(int.TryParse( Regex.Match(arguments, @"(?<=--port[\s=]+)\d+", RegexOptions.RightToLeft).Value, out var port)){
            return port;
        }return null;
    }
    
    private string CreateLlamaArgument(LLMModel model){
        var arguments = new StringBuilder();
        arguments.Append($"-m \"{model.FilePath}\" ");
        arguments.Append($"--ctx-size {model.Context} ");
        arguments.Append($"--port {_llamaConfig.Port} ");
        arguments.Append(_llamaConfig.OtherSettings);
        _activePort = GetPort(model.Arguments) ?? _llamaConfig.Port;
        
        // Overrides any default arguments
        arguments.Append($" {model.Arguments} ");
        return arguments.ToString();
    }

    
    /// <summary>Starts the Llama-server process</summary>
    /// <param name="model">The LLM Model object returned from the database / llmmodel endpoint</param>
    /// <remarks>Opens an external terminal that then runs Llama-server to circumvent it closing on errors </remarks>
    public void StartServer(LLMModel model){
        
        // Only one server instance should run at a time
        if(!_process.NotRunning) _process.Stop();

        var (file, args) = OpenTerminalHelper.CreateOSTerminalCommand(OpenTerminalHelper.CurrentOS, _llamaConfig.Path, CreateLlamaArgument(model) + OpenTerminalHelper.CreateLogFilter(OpenTerminalHelper.CurrentOS));
        _process.Start(file, args);
    }

    /// <summary>Stop Llama Server process</summary>
    public void StopServer(){
        if(_process == null || _process.NotRunning) return;
        _process.Stop();
    }

    /// <summary>Get current running state of Llama</summary>
    /// <returns>
    /// <see cref="ServerState.Online"/> If the server is ready
    /// <see cref="ServerState.Loading"/> If the server is booting up
    /// <see cref="ServerState.Offline"/> If the server is not running
    ///</returns>
    public async Task<ServerState> RunningP() {
        if(_process.NotRunning) return ServerState.Offline;

        try{
            using var timeout = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(1));
            var response = await _client.GetAsync($"http://127.0.0.1:{_activePort}/health", timeout.Token);

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

    /// <summary>Check if llama is generating a prompt</summary>
    /// <returns> true if llama is generating a prompt, false otherwise</returns>
    public async Task<bool> GeneratingP(){
        if (await RunningP() != ServerState.Online) return false;
        
        var json = await _client.GetStringAsync($"http://localhost:{_activePort}/slots");
        if (string.IsNullOrWhiteSpace(json)) return false;

        return JsonSerializer.Deserialize<JsonElement[]>(json)!
            .Any(s => s.GetProperty("is_processing").GetBoolean());
    }
}
