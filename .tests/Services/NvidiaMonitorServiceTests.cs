// NvidiaMonitorServiceTests.cs

using Microsoft.Extensions.Options;
using LLMRemote.Options;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Tests.Factories;
using Bogus;
using LLMRemote.Models;
using System.Net;
using LLMRemote.Util;

namespace LLMRemote.Tests;

public class NvidiaMonitorServiceTests{
    private readonly NvidiaMonitorService _service;
    private readonly FakeProcess _process = new();
    private readonly Faker _faker = new();

    public NvidiaMonitorServiceTests(){
        _service = new NvidiaMonitorService(_process);
    }

    [Fact]
    public void Constructor_StartNvidiaSmi(){
        Assert.Equal(1, _process.StartCount);
        Assert.Equal("nvidia-smi", _process.FileName);
        Assert.Equal("--query-gpu=name,utilization.gpu,memory.used,memory.total --format=csv,noheader,nounits -l 1", _process.Arguments);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void GetUsage_GPUData(int count){
        var gpus = Enumerable.Range(0, count).Select(i =>{
            var totalMemory = _faker.PickRandom(new[] { 2048, 4096, 8192, 12288, 16384 });

            return new {
                Name = $"{_faker.Commerce.ProductName()} {i}",
                Gpu = _faker.Random.Int(0, 100).ToString(),
                Memory = _faker.Random.Int(0, totalMemory).ToString(),
                TotalMemory = totalMemory,
            };
        }).ToList();

        foreach (var g in gpus) _process.Output!($"{g.Name}, {g.Gpu}, {g.Memory}, {g.TotalMemory}");

        var result = _service.GetUsage();

        Assert.Equal(count, result.Count);

        foreach (var g in gpus){
            Assert.Equal(g.Gpu, result[g.Name]["gpu"]);
            Assert.Equal(g.Memory, result[g.Name]["memory"]);
        }
    }

    [Fact]
    public void GetUsage_SameGpu_OverwritesValues(){
        var name = _faker.Commerce.ProductName();

        _process.Output!($"{name}, 10, 512, 2048");
        _process.Output!($"{name}, 90, 4096, 4096");

        var result = _service.GetUsage();

        Assert.Single(result);
        Assert.Equal("90", result[name]["gpu"]);
        Assert.Equal("4096", result[name]["memory"]);
    }

    [Fact]
    public void GetUsage_NoOutput_ReturnsEmpty(){
        var result = _service.GetUsage();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("NVIDIA-SMI has failed because it couldn't communicate with the NVIDIA driver.")]
    [InlineData("No devices were found")]
    [InlineData("RTX 4090, 45")]
    public void GetUsage_InvalidLine_NotAdded(string line){
        _process.Output!(line);

        var result = _service.GetUsage();

        Assert.Empty(result);
    }
}
