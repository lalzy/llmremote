// LLMModelsFactory.cs


using AutoBogus;
using LLMRemote.Models;
using LLMRemote.Data;

namespace LLMRemote.Tests.Factories;

public class LLMModelFactory{
    public static LLMModel Create(AppDbContext db){
        var model = AutoFaker.Generate<LLMModel>();

        db.LLMModel.Add(model);
        db.SaveChanges();
        return model;
    }

    public static LLMModel Create(DatabaseFixture fixture){
        using var db = fixture.CreateContext();
        return Create(db);
    }
}
