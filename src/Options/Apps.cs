// Apps.cs

namespace LLMRemote.Options;

public class Apps
{
    public AppConfig Llama { get; set; } = new();
    public AppConfig ComfyUI { get; set; } = new();
}

public class AppConfig
{
    public string Path { get; set; } = "";
    public int Port { get; set; }
}
