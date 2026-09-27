// IProcessManager.cs

using System;
using System.Diagnostics;
using System.ComponentModel;
using LLMRemote.Util;

namespace LLMRemote.Services;

public interface IManagedProcess : IDisposable{
    bool NotRunning { get; }
    
    ///<summary>Start a process</summary>
    ///<param name="fileName">The process filename + path</param>
    ///<param name="arguments">The arguments to give to the process</param>
    ///<remarks>Assigned to Job which kills process and its children on parent close</remarks>
    void Start(string fileName, string arguments);
    
    ///<summary>Stop process</summary>
    void Stop();
}


public class ManagedProcess : IManagedProcess{
    private Process? _process;

    public bool NotRunning => _process == null || _process.HasExited;
    
    /// <inheritdoc/>
    public void Start(string fileName, string arguments){
        _process = Process.Start(new ProcessStartInfo(fileName, arguments){
           UseShellExecute = true     
        });
        if (_process != null) JobObject.Assign(_process);
    }
    
    /// <inheritdoc/>
    public void Stop(){
        if(_process == null) return;
        try{
            if(!NotRunning){
                _process!.Kill(entireProcessTree: true);
                _process.WaitForExit();
            }
            
        }catch (InvalidOperationException){
            // Do nothing
        }catch (Win32Exception){
            // Do nothing
        }finally{
            _process.Dispose();
            _process = null;
        }
    }

    public void Dispose(){
        Stop();
    }
}
