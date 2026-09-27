// OpenTerminalHelper.cs

namespace LLMRemote.Util;

public static class OpenTerminalHelper
{

    ///<summary>Assigned at compile time. Used to allow testing of CreateOSTerminalCommand</summary>
    public const HostOS CurrentOS =
#if WINDOWS
        HostOS.Windows;
#elif LINUX
    HostOS.Linux;
#elif MAC
    HostOS.Mac;
#endif
    
    /// <summary>Creates OS-specific terminal command for running llama-server</summary>
    /// <param name="os">Current operating system</param>
    /// <param name="path">path to Llama-server</param>
    /// <param name="arguments">Arguments to llama.</param>
    /// <returns>A tuple of the terminal to run (<c>fileName</c>) and its arguments (<c>arguments</c>)</returns>
    public static (string fileName, string arguments) CreateOSTerminalCommand(HostOS os, string path, string arguments) => os switch
    {
        HostOS.Windows => ("cmd.exe", $"/k \"\"{path}\" {arguments}\""),
        HostOS.Linux => ("bash", $"-c \"'{path}' {arguments}; exec bash\""),
        HostOS.Mac => ("zsh", $"-c \"'{path}' {arguments}; exec zsh\""),
    };
}
