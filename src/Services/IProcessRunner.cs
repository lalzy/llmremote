namespace LLMRemote.Services;

public interface IProcessRunner{
    IRunningProcess Start(string filename, string arguments);
}

public interface IRunningProcess{
    bool HasExited { get; }
    void Stop();
}
