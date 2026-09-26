// Apps.cs

namespace LLMRemote.Options;

public class Apps
{
    public LlamaConfig Llama { get; set; } = new();
    public AppConfig ComfyUI { get; set; } = new();
}


public class AppConfig
{
    public string Path { get; set; } = "";
    public int Port { get; set; }
}

public class LlamaConfig : AppConfig{
    public string OtherSettings { get; set; } = "";
}
