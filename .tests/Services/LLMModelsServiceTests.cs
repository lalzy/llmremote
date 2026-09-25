// LLMModelsServiceTests.cs

using Bogus;
using AutoBogus;
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
}
