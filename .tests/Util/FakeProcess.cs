// FakeProcess.cs

using LLMRemote.Services;

namespace LLMRemote.Tests.Util;

public class FakeProcessRunner : IProcessRunner{
    public int StartCount { get; private set; }
    public string? FileName { get; private set; }
    public string? Arguments { get; private set; }
    public FakeRunningProcess? LastProcess { get; private set; }

    public IRunningProcess Start(string fileName, string arguments){
        StartCount++;
        FileName = fileName;
        Arguments = arguments;
        LastProcess = new FakeRunningProcess();
        return LastProcess;
    }
}

public class FakeRunningProcess : IRunningProcess{
    public bool HasExited { get; private set; }

    public void Stop(){
        HasExited = true;
    }
}
