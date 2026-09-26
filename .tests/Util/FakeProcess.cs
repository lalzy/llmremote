// FakeProcess.cs

using System;
using LLMRemote.Services;

namespace LLMRemote.Tests.Util;

public class FakeProcess : IManagedProcess
{
    public int StartCount { get; private set; }
    public int StopCount { get; private set; }
    public bool HasExited { get; private set; } = true;

    public void Start(string fileName, string arguments)
    {
        StartCount++;
        HasExited = false;
    }

    public void Stop()
    {
        StopCount++;
        HasExited = true;
    }

    public void Dispose()
    {
        // Nothing to clean up in a fake.
    }
}
