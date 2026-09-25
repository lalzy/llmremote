// LLamaService.cs
using Microsoft.Extensions.Options;
using LLMRemote.Data;
using LLMRemote.Options;
namespace LLMRemote.Services;

public class LlamaService(AppDbContext db, IManagedProcess process, IOptions<Apps> apps){
    private readonly AppDbContext _db = db;
    private readonly IManagedProcess _process = process;
    private readonly AppConfig _llamaConfig = apps.Value.Llama;

    public void StartServer(Guid modelID){
        // Only one server instance should run at a time
        if(!_process.HasExited) _process.Stop();
        var arguments = "";
        
        // running through CMD to prevent llama closing on errors.
        _process.Start("cmd.exe", $"/k \"\"{_llamaConfig.Path}\" {arguments}\"");
    }

    public void StopServer(){
        _process.Stop();
    }
}
