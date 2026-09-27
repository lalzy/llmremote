// LLMModelsService.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using LLMRemote.Models;
using LLMRemote.Data;
using LLMRemote.Util;

namespace LLMRemote.Services;

public class LLMModelsService(AppDbContext db){
    private readonly AppDbContext _db = db;
    
    ///<summary>Create a new model entry</summary>
    ///<param name="request">The LLMModel Request with model data</param>
    ///<returns>Newly created model</returns>
    public LLMModel Add(LLMModelRequest request){
        LLMModel model = request.ConvertModelToDTO<LLMModel>();
        var ret = _db.LLMModel.Add(model);
        _db.SaveChanges();
        
        return model;
    }

    ///<summary>Get a model</summary>
    ///<param name="ID">Model ID to fetch</param>
    ///<throws cref="KeyNotfoundexception">If the model doesn't exist</throws>
    ///<returns>The Model</returns>
    public LLMModel Get(Guid ID){
        var model = _db.LLMModel.FirstOrDefault(m => m.ID == ID);
        if(model == null) throw new KeyNotFoundException("Model not found");
        return model;
    }

    ///<summary>Order options</summary>
    public enum OrderBy{
        Name, Created, Updated, ID
    }
    
    
    ///<summary>Get all models paginated</summary>
    ///<param name="page">Which page to get</param>
    ///<param name="count">How many per page</param>
    ///<param name="orderBy">What to order results by</param>
    ///<returns>Page as list</returns>
    public List<LLMModel> GetAll(int page=1, int count=10, OrderBy orderBy=OrderBy.Name){
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
    
    ///<summary>Replace model data in Database</summary>
    ///<param name="ID">model ID</param>
    ///<param name="request">The model Request used to replace old data</param>
    ///<returns>Return the new model object</returns>
    public LLMModel Update(Guid ID, LLMModelRequest request){
        LLMModel model = request.ConvertModelToDTO<LLMModel>();
        model.ID = ID;
        _db.LLMModel.Update(model);
        _db.SaveChanges();
        return model;
    }

    ///<summary>Delete model from Database</summary>
    ///<param name="id">model ID</param>
    public void Delete(Guid id){
        _db.LLMModel.Where(m => m.ID == id).ExecuteDelete();
    }
}
