// LLMModelsFactory.cs


using AutoBogus;
using LLMRemote.Models;
using LLMRemote.Data;

namespace LLMRemote.Tests.Factories;

public class LLMModelFactory{
    public static LLMModel Create(AppDbContext db, int? port=null){
        var model = AutoFaker.Generate<LLMModel>();
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;
        if(port.HasValue) model.Arguments += $" --port {port.Value}";
        
        db.LLMModel.Add(model);
        db.SaveChanges();
        return model;
    }

    public static LLMModel Create(DatabaseFixture fixture, int? port=null){
        using var db = fixture.CreateContext();
        return Create(db, port);
    }
}
