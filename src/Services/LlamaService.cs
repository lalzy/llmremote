// LLamaService.cs
using Microsoft.Extensions.Options;
using System.Text;
using LLMRemote.Options;
using LLMRemote.Models;

namespace LLMRemote.Services;

public class LlamaService([FromKeyedServices("llama")] IManagedProcess process, IOptions<Apps> apps){
    private readonly IManagedProcess _process = process;
    private readonly LlamaConfig _llamaConfig = apps.Value.Llama;

    private string CreateLlamaArgument(LLMModel model){
        var arguments = new StringBuilder();
        arguments.Append($"-m \"{model.FilePath}\" ");
        arguments.Append($"--ctx-size {model.Context} ");
        arguments.Append($"--port {_llamaConfig.Port} ");
        arguments.Append(_llamaConfig.OtherSettings);
        return arguments.ToString();
    }
    
    public void StartServer(LLMModel model){
        
        // Only one server instance should run at a time
        if(!_process.HasExited) _process.Stop();
        
        // running through CMD to prevent llama closing on errors.
        _process.Start("cmd.exe", $"/k \"\"{_llamaConfig.Path}\" {CreateLlamaArgument(model)}\"");
    }

    public void StopServer(){
        _process.Stop();
    }
}
