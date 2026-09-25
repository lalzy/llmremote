// LLamaService.cs
using LLMRemote.Data;

namespace LLMRemote.Services;

public class LlamaService(AppDbContext db, IManagedProcess process){
    private readonly AppDbContext _db = db;
    private readonly IManagedProcess _process = process;

    public void StartServer(string pathToLlama, Guid modelID){
        // Only one server instance should run at a time
        if(!_process.HasExited) _process.Stop();
        _process.Start(pathToLlama, "Some argument");
    }

    public void StopServer(){
        _process.Stop();
    }
}
