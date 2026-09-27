// ShutdownTimerServiceTests.cs

using System;
using Bogus;
using System.Net;
using Microsoft.Extensions.Time.Testing;
using LLMRemote.Services;
using LLMRemote.Tests.Util;
using LLMRemote.Tests.Factories;
using LLMRemote.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace LLMRemote.Tests;

public class ShutdownTimerServiceTests:DatabaseTestBase{
    private readonly Faker _faker = new Faker();
    private readonly DateTimeOffset _now;
    private readonly FakeTimeProvider _timeProvider;
    private readonly ShutdownTimerService _service;
    private readonly LlamaService _llama;
    private readonly FakeProcess _process;

    public ShutdownTimerServiceTests(DatabaseFixture fixture) : base (fixture){
        _process = new FakeProcess();
        _llama = new LlamaService(_process, new OptionsWrapper<Apps>(new Apps {Llama = new LlamaConfig {
                        Path = _faker.System.FilePath(), Port = _faker.Internet.Port(), OtherSettings =""
                        }}), new HttpClient(new FakeHttpHandler(HttpStatusCode.OK)));
        _now = _faker.Date.RecentOffset();
        _timeProvider = new FakeTimeProvider(_now);

        // get keyed LLamaService
        var scopeFactory = new ServiceCollection()
            .AddScoped<LlamaService>(_ => _llama)
            .BuildServiceProvider()
            .GetRequiredService<IServiceScopeFactory>();

        _service = new ShutdownTimerService(_timeProvider, scopeFactory);
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
    public void Set_StopsLlamaWhenTimeElapses(){
        _llama.StartServer(LLMModelFactory.Create(_fixture));
        var duration = _faker.Random.Int(1, 3600);
        _service.Set(duration);

        _timeProvider.Advance(TimeSpan.FromSeconds(duration));
        Assert.Equal(1, _process.StopCount);
    }

    [Fact]
    public void Set_ParrallelCallsLaveOnlyOneTimer(){
        var duration = _faker.Random.Int(1, 3600);
        var threadCount = Environment.ProcessorCount * 2;
        var barrier = new Barrier(threadCount);

        var threads = Enumerable.Range(0, threadCount)
            .Select(_ => new Thread(() => {
                barrier.SignalAndWait();
                for (var i = 0; i < 1000; i++) _service.Set(duration);
            })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        _timeProvider.Advance(TimeSpan.FromSeconds(duration));
        Assert.Equal(1, _process.StopCount);
    }

    [Fact]
    public void Cancel_ClearShutdownAt(){
        _service.Set(_faker.Random.Int(1, 3600));
        _service.Cancel();
        Assert.Null(_service.ShutdownAt);
    }

    [Fact]
    public void Cancel_StopsTimer(){
        var duration = _faker.Random.Int(1, 3600);
        _service.Set(duration);
        _service.Cancel();

        _timeProvider.Advance(TimeSpan.FromSeconds(duration));
        Assert.Equal(0, _process.StopCount);
    }

    [Fact]
    public void SetAndCancel_ParallelCallsStayConsistent(){
        var duration = _faker.Random.Int(1, 3600);

        for (var round = 0; round < 5000; round++){
            var barrier = new Barrier(2);
            var set = new Thread(() => { barrier.SignalAndWait(); _service.Set(duration); });
            var cancel = new Thread(() => { barrier.SignalAndWait(); _service.Cancel(); });

            set.Start(); cancel.Start();
            set.Join(); cancel.Join();

            var expected = _service.ShutdownAt is null ? 0 : 1;
            var before = _process.StopCount;
            _timeProvider.Advance(TimeSpan.FromSeconds(duration));

            Assert.Equal(expected, _process.StopCount - before);
        }
    }
}
