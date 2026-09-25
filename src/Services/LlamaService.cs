// LLamaService.cs
using LLMRemote.Data;

namespace LLMRemote.Services;

public class LlamaService(AppDbContext db){
    private readonly AppDbContext _db = db;

    public void StartServer(Guid modelID){
        
    }
}
