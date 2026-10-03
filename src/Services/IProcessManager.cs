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

    /// <summary>Start a process without opening external</summary>
    /// <param name="fileName"><inheritdoc cref="Start" path="/param[@name='fileName']"/></param>
    /// <param name="arguments"><inheritdoc cref="Start" path="/param[@name='arguments']"/></param>
    /// <param name="onOutput">Called for every line the process writes</param>
    /// <remarks><inheritdoc cref="Start" path="/remarks"/></remarks>
    void StartWithOutput(string fileName, string arguments, Action<string> onAction);
    
    ///<summary>Stop process</summary>
    void Stop();
}


public class ManagedProcess : IManagedProcess{
    private Process? _process;

    public bool NotRunning => _process == null || _process.HasExited;
    
    /// <inheritdoc/>
    public void Start(string fileName, string arguments){
        Stop();
        _process = Process.Start(new ProcessStartInfo(fileName, arguments){
           UseShellExecute = true     
        });
        if (_process != null) JobObject.Assign(_process);
    }

    /// <inheritdoc/>
    public void StartWithOutput(string fileName, string arguments, Action<string> onOutput){
        Stop();
        _process = new Process{
            StartInfo = new ProcessStartInfo(fileName, arguments){
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };

        _process.OutputDataReceived += (_, e) => {
            if(e.Data != null) onOutput(e.Data);
        };

        _process.Start();
        JobObject.Assign(_process);
        _process.BeginOutputReadLine();
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
