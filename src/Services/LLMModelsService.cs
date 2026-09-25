// LLMModelsService.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using LLMRemote.Models;
using LLMRemote.Data;
using LLMRemote.Util;

namespace LLMRemote.Services;

public class LLMModelsService(AppDbContext db){
    private readonly AppDbContext _db = db;
    
    public LLMModel Add(LLMModelRequest request){
        LLMModel model = request.ConvertModelToDTO<LLMModel>();
        var ret = _db.LLMModel.Add(model);
        _db.SaveChanges();
        
        return model;
    }

    public LLMModel Get(Guid ID){
        var model = _db.LLMModel.FirstOrDefault(m => m.ID == ID);
        return model;
    }

    public enum OrderBy{
        Name, Created, Updated, ID
    }
    
    public List<LLMModel> GetAll(int page=1, int count=1, OrderBy orderBy=OrderBy.Name){
        var request = _db.LLMModel.AsNoTracking();

        request = orderBy switch
        {
            OrderBy.ID => request.OrderBy(m => m.ID),
            OrderBy.Name => request.OrderBy(m => EF.Functions.Collate(m.Name, "NOCASE")).ThenBy(m => m.ID),
            OrderBy.Created => request.OrderBy(m => m.CreatedAt).ThenBy(m => m.ID),
            OrderBy.Updated => request.OrderBy(m => m.UpdatedAt).ThenBy(m => m.ID),
        };

        return request.Skip((page - 1) * count).Take(count).ToList();
    }
    
    public LLMModel Edit(LLMModelRequest request){
        LLMModel model = request.ConvertModelToDTO<LLMModel>();
        _db.LLMModel.Update(model);
        return model;
    }
}
