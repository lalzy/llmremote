// ShutdownTimerServiceTests.cs

using System;
using Bogus;
using System.Net;
using Microsoft.Extensions.Time.Testing;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Options;
using Microsoft.Extensions.Options;

namespace LLMRemote.Tests;

public class ShutdownTimerServiceTests{
    private readonly Faker _faker = new Faker();
    private readonly DateTimeOffset _now;
    private readonly FakeTimeProvider _timeProvider;
    private readonly ShutdownTimerService _service;
    private readonly LlamaService _llama;
    private readonly FakeProcess _process;

    public ShutdownTimerServiceTests(){
        _llama = new LlamaService(new FakeProcess(),
                                  new OptionsWrapper<Apps>(new Apps {Llama = new LlamaConfig { Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings ="" }}),
                                  new HttpClient(new FakeHttpHandler(HttpStatusCode.OK)));
        _now = _faker.Date.RecentOffset();
        _timeProvider = new FakeTimeProvider(_now);
        _service = new ShutdownTimerService(_timeProvider, _llama);
    }

    [Fact]
    public void Set_SetsShutdownAtTime(){
        var duration = _faker.Random.Int(1, 3600);
        _service.Set(duration);
        Assert.Equal(_now.AddSeconds(duration), _service.ShutdownAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Set_LessThanOneThrows(int duration){
        Assert.Throws<ArgumentException>(() => _service.Set(duration));
    }

    [Fact]
    public void Set_NewSetReplaceOldTimer(){
        var first = _faker.Random.Int(1, 50);
        var second = _faker.Random.Int(51, 100);
        _service.Set(first);
        _service.Set(second);
        _timeProvider.Advance(TimeSpan.FromSeconds(first));
        Assert.Equal(_now.AddSeconds(second), _service.ShutdownAt);
    }

    [Fact]
    public void Sets_DurationContinueFromNewSet(){
        _service.Set(3600);
        _timeProvider.Advance(TimeSpan.FromMinutes(30));
        _service.Set(3600);
        Assert.Equal(_now.AddMinutes(90), _service.ShutdownAt);
    }

    [Fact]
    public void Set_ClearsShutdownAtWhenTimeElapses(){
        var duration = _faker.Random.Int(1, 3600);
        _service.Set(duration);

        _timeProvider.Advance(TimeSpan.FromSeconds(duration));

        Assert.Null(_service.ShutdownAt);
    }

    [Fact]
    public void Cancel_ClearShutdownAt(){
        _service.Set(_faker.Random.Int(1, 3600));
        _service.Cancel();
        Assert.Null(_service.ShutdownAt);
    }
}
