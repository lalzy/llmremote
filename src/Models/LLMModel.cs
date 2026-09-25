// LLMModel.cs
using System.ComponentModel.DataAnnotations;

namespace LLMRemote.Models;

public class LLMModel
{
    public Guid ID { get; set; }
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public int? context { get; set; }
}
