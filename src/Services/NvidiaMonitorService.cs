// NvidiaMonitorService.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using LLMRemote.Options;
using LLMRemote.Models;
using LLMRemote.Util;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LLMRemote.Services;

public class NvidiaMonitorService{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _gpus = new();

    public NvidiaMonitorService([FromKeyedServices("nvidia")] IManagedProcess process){
        process.StartWithOutput("nvidia-smi", "--query-gpu=name,utilization.gpu,memory.used --format=csv,noheader,nounits -l 1",
        OnOutput);
    }

    private void OnOutput(string line){
        var v = line.Split(',');
        if (v.Length < 3) return;
        var name = v[0].Trim();
        _gpus[name] = new Dictionary<string, string>
        {
            ["gpu"] = v[1].Trim(),
            ["memory"] = v[2].Trim()
        };
    }
    
    public ConcurrentDictionary<string, Dictionary<string, string>> GetUsage(){
        return _gpus;
    }
}
