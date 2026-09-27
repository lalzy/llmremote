// FakeProcess.cs

using System;
using LLMRemote.Services;

namespace LLMRemote.Tests.Util;

public class FakeProcess : IManagedProcess
{
    public int StartCount { get; private set; }
    public int StopCount { get; private set; }
    public bool NotRunning { get; private set; } = true;
    public string FileName { get; private set; } = "";
    public string Arguments { get; private set; } = "";

    public void Start(string fileName, string arguments)
    {
        StartCount++;
        NotRunning = false;
        FileName = fileName;
        Arguments = arguments;
    }

    public void Stop()
    {
        StopCount++;
        NotRunning = true;
    }

    public void Dispose()
    {
        // Nothing to clean up in a fake.
    }
}
