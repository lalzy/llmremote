// IProcessManager.cs

using System.Diagnostics;

namespace LLMRemote.Services;

public interface IManagedProcess{
    bool HasExited { get; }
    void Start(string filename, string arguments);
    void Stop();
}

public class ManagedProcess : IManagedProcess{
    private Process? _process;

    public bool HasExited => _process == null || _process.HasExited;

    public void Start(string fileName, string arguments){
        _process = Process.Start(new ProcessStartInfo(fileName, arguments){
           UseShellExecute = true     
        });
    }

    public void Stop(){
        if(!HasExited){
            _process!.Kill(entireProcessTree: true);
            _process.WaitForExit();
        }
        _process = null;
    }
}
