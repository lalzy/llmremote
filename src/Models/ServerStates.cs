namespace LLMRemote.Models;

///<summary>State of external process, such as LLama-server</summary>
public enum ServerState{
    Offline,
    Loading,
    Online
}
