// LLMModelsServiceTests.cs

using Bogus;
using AutoBogus;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Linq;
using LLMRemote.Tests.Util;
using LLMRemote.Services;
using LLMRemote.Models;
using LLMRemote.Tests.Factories;

namespace LLMRemote.Tests;

public class LLMModelsServiceTests : DatabaseTestBase{
    private readonly LLMModelsService _service;
    private readonly Faker _faker = new Faker();

    public LLMModelsServiceTests(DatabaseFixture fixture) : base(fixture){
        _service = new LLMModelsService(fixture.CreateContext());
    }

    // Helpers
    private int GetPageCount(int total, int fetchCount){
        return (int)Math.Ceiling(total / (double)fetchCount);
    }


    // Tests

    [Fact]
    public void AddModel_ReturnModel(){
        var request = AutoFaker.Generate<LLMModelRequest>();
        var model = _service.Add(request);
    
        Assert.Equivalent(request, model);
    }

    [Fact]
    public void AddModel_WritesToDb(){
        var request = AutoFaker.Generate<LLMModelRequest>();
        var model = _service.Add(request);

        var exists = _fixture.CreateContext().LLMModel.Any(m => m.ID == model.ID);

        Assert.True(exists);
    }

    [Fact]
    public void AddModel_CreatedAtSet(){
        var before = DateTime.UtcNow;
        var model = _service.Add(AutoFaker.Generate<LLMModelRequest>());
        var after = DateTime.UtcNow;

        Assert.InRange(model.CreatedAt, before, after);
    }

    [Fact]
    public void GetModel_ReturnsModel(){
        var expected = LLMModelFactory.Create(_fixture.CreateContext());

        var model = _service.Get(expected.ID);

        Assert.NotNull(model);
        Assert.Equivalent(expected, model);
    }

    [Fact]
    public void GetModel_ReturnsCorrectModel(){
        var models = Enumerable.Range(0, 5).Select(_ => LLMModelFactory.Create(_fixture.CreateContext())).ToList();
        foreach(var expected in models){
            var model = _service.Get(expected.ID);

            Assert.Equivalent(expected, model);
        }
    }

    [Fact]
    public void GetModel_DoesNotExistThrows(){
        Assert.Throws<KeyNotFoundException>(() => _service.Get(_faker.Random.Guid()));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    public void GetAll_GetRange(int fetchCount){
        var models = Enumerable.Range(0, 100).Select(_ => LLMModelFactory.Create(_fixture.CreateContext())).ToList();
        var fetched = _service.GetAll(count:fetchCount);
        Assert.Equal(fetchCount, fetched.Count);
    }

    [Fact]
    public void GetAll_NoPageOverlap(){
        var models = Enumerable.Range(0, 100).Select(_ => LLMModelFactory.Create(_fixture.CreateContext())).ToList();
        var page1 = _service.GetAll(page: 1, count: 25);
        var page2 = _service.GetAll(page: 2, count: 25);

        Assert.Empty(page1.Select(m => m.ID).Intersect(page2.Select(m => m.ID)));
    }

    [Fact]
    public void GetAll_NoDataMissing(){
        const int TOTAL = 100;
        const int FETCHCOUNT = 25;
        
        var models = Enumerable.Range(0, TOTAL).Select(_ => LLMModelFactory.Create(_fixture.CreateContext())).ToList();
        var fetched = new List<LLMModel>();

        for(int page = 1; page <= GetPageCount(TOTAL, FETCHCOUNT); page++){
            fetched.AddRange(_service.GetAll(page: page, count: FETCHCOUNT));
        }

        Assert.Equal(models.Select(m => m.ID).OrderBy(ID => ID), fetched.Select(m => m.ID).OrderBy(ID => ID));
    }

    [Fact]
    public void GetAll_lastPageIspartial(){
        const int TOTAL = 100;
        const int FETCHCOUNT = 30;
        var models = Enumerable.Range(0, TOTAL).Select(_ => LLMModelFactory.Create(_fixture.CreateContext())).ToList();
        var lastPage = _service.GetAll(page: GetPageCount(TOTAL, FETCHCOUNT), count: FETCHCOUNT);
        Assert.Equal(TOTAL % FETCHCOUNT, lastPage.Count);
    }

    [Theory]
    [InlineData(LLMModelsService.OrderBy.ID)]
    [InlineData(LLMModelsService.OrderBy.Name)]
    [InlineData(LLMModelsService.OrderBy.Created)]
    [InlineData(LLMModelsService.OrderBy.Updated)]
    public void GetAll_OrderedCorrectly(LLMModelsService.OrderBy orderBy){
        const int TOTAL = 100;
        var models = Enumerable.Range(0, TOTAL).Select(_ => LLMModelFactory.Create(_fixture.CreateContext())).ToList();
        var fetched = _service.GetAll(count:TOTAL, orderBy:orderBy);

        Func<LLMModel, object> key = orderBy switch {
            LLMModelsService.OrderBy.ID      => m => m.ID,
            LLMModelsService.OrderBy.Name    => m => m.Name,
            LLMModelsService.OrderBy.Created => m => m.CreatedAt,
            LLMModelsService.OrderBy.Updated => m => m.UpdatedAt,
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy))
        };

        Assert.Equal(models.Select(key).OrderBy(k => k), fetched.Select(key));
    }

    [Fact]
    public void Update_ReturnsModel(){
        var model = LLMModelFactory.Create(_fixture.CreateContext());
        var request = AutoFaker.Generate<LLMModelRequest>();
        
        var fetched = _service.Update(model.ID, request);

        Assert.Equivalent(request, fetched);
    }

    private LLMModelRequest CreateRequestFromModel(LLMModel model){
        return new LLMModelRequest{
            Name = model.Name,
            FilePath = model.FilePath,
            Context = model.Context,
        };
    }

    [Theory]
    [InlineData(nameof(LLMModelRequest.Name))]
    [InlineData(nameof(LLMModelRequest.FilePath))]
    [InlineData(nameof(LLMModelRequest.Context))]
    public void Update_OnlySelectedFieldIsChanged(string propertyName){
        var model = LLMModelFactory.Create(_fixture.CreateContext());

        var request = CreateRequestFromModel(model);

        // change property value to new random
        PropertyInfo property = typeof(LLMModelRequest).GetProperty(propertyName)!;
        Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        object value = type switch{
            _ when type == typeof(string) => _faker.Random.String2(10),
            _ when type == typeof(int) => _faker.Random.Int(1, int.MaxValue),
            _ => throw new NotSupportedException(type.Name)
        };

        property.SetValue(request, value);

        _service.Update(model.ID, request);
        var dbFetch = _fixture.CreateContext().LLMModel.FirstOrDefault(m => m.ID == model.ID);
        
        Assert.Equivalent(request, dbFetch);
    }
    
    [Fact]
    public void UpdateModel_UpdatedAtSet(){
        var model = LLMModelFactory.Create(_fixture.CreateContext());
        
        var before = DateTime.UtcNow;
        var fetched = _service.Update(model.ID,  CreateRequestFromModel(model));
        var after = DateTime.UtcNow;

        Assert.InRange(fetched.UpdatedAt, before, after);
    }

    [Fact]
    public void UpdateModel_DoesNotChangeCreatedAt(){
        var model = LLMModelFactory.Create(_fixture.CreateContext());
        _service.Update(model.ID, CreateRequestFromModel(model));

        var fetched = _fixture.CreateContext().LLMModel.FirstOrDefault(m => m.ID == model.ID);
        Assert.Equal(model.CreatedAt, fetched.CreatedAt);
    }
    
    [Fact]
    public void UpdateModel_ThrowsOnNotFound(){
        Assert.Throws<DbUpdateConcurrencyException>(() => _service.Update(_faker.Random.Guid(), AutoFaker.Generate<LLMModelRequest>()));
    }

    [Fact]
    public void DeleteModel_Success(){
        var model = LLMModelFactory.Create(_fixture.CreateContext());
        _service.Delete(model.ID);

        var dbFetch = _fixture.CreateContext().LLMModel.FirstOrDefault(m => m.ID == model.ID);
        Assert.Null(dbFetch);
    }
}
