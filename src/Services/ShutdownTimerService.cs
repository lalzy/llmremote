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

    public ShutdownTimerService(TimeProvider timeProvider, LlamaService llama)
    {
        _timeProvider = timeProvider;
        _llama = llama;
    }

    /// <summary>Cancel shutdown timer</summary>
    public void Cancel()
    {
        ShutdownAt = null;
    }

    /// <summary>Set the time to shutdown</summary>
    /// <param name="duration">Time in seconds until shutdown</param>
    public void Set(int duration)
    {
        if(duration < 1) throw new ArgumentException("must be at least 1 second");
        ShutdownAt = _timeProvider.GetUtcNow().AddSeconds(duration);
        _timer?.Dispose();
        _timer = _timeProvider.CreateTimer(_ => ShutdownAt = null, null, TimeSpan.FromSeconds(duration), Timeout.InfiniteTimeSpan);
    }

    /// <summary>Trigger shutdown of processes</summary>
    private void Fire()
    {
    }

    /// <summary>Cleanup of the thread after shutting down processes</summary>
    private void Clear()
    {
    }
}
