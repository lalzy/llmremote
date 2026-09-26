// LLamaService.cs
using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Models;

namespace LLMRemote.Services;

public class LlamaService(IManagedProcess process, IOptions<Apps> apps){
    private readonly IManagedProcess _process = process;
    private readonly AppConfig _llamaConfig = apps.Value.Llama;

    
    public void StartServer(LLMModel model){
        
        // Only one server instance should run at a time
        if(!_process.HasExited) _process.Stop();
        var arguments = $"-m {model.FilePath} --ctx-size {model.Context}";
        
        // running through CMD to prevent llama closing on errors.
        _process.Start("cmd.exe", $"/k \"\"{_llamaConfig.Path}\" {arguments}\"");
    }

    public void StopServer(){
        _process.Stop();
    }
}
