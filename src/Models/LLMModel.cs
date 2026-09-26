// LLMModel.cs
using System.ComponentModel.DataAnnotations;

namespace LLMRemote.Models;

public class LLMModel{
    public Guid ID { get; set; }
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public int? Context { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LLMModelRequest{
    [Required]
    public string Name {get; set;}
    
    [Required]
    public string FilePath {get; set;}
    
    [Range(1, int.MaxValue)]
    public int? Context {get; set;}
}
