// ShutdownTimerService.cs

using System;
using System.Threading;
using LLMRemote.Services;

namespace LLMRemote.Services;

public class ShutdownTimerService
{
    private readonly LlamaService _llama;
    private readonly TimeProvider _timeProvider;
    public DateTimeOffset? ShutdownAt { get; private set; }
    private readonly object _lock = new();
    private ITimer? _timer;
    private object? _token;

    public ShutdownTimerService(TimeProvider timeProvider, LlamaService llama)
    {
        _timeProvider = timeProvider;
        _llama = llama;
    }

    /// <summary>Cancel shutdown timer</summary>
    public void Cancel()
    {
        lock(_lock){
            ShutdownAt = null;
            _timer?.Dispose();
        }
    }

    /// <summary>Set the time to shutdown</summary>
    /// <param name="duration">Time in seconds until shutdown</param>
    public void Set(int duration)
    {
        if(duration < 1) throw new ArgumentException("must be at least 1 second");
        lock(_lock){
            ShutdownAt = _timeProvider.GetUtcNow().AddSeconds(duration);
            _timer?.Dispose();
            var token = new object();
            _timer = _timeProvider.CreateTimer(_ => Fire(token), null, TimeSpan.FromSeconds(duration), Timeout.InfiniteTimeSpan);
            
        }
    }

    /// <summary>Trigger shutdown of processes</summary>
    private void Fire(object token)
    {
        lock(_lock){
            if(token != _token) return;
            Clear();
            _llama.StopServer();
        }
    }

    /// <summary>Cleanup of the thread after shutting down processes</summary>
    private void Clear()
    {
        ShutdownAt = null;
        _timer?.Dispose();
        _timer = null;
        _token = null;
    }
}
