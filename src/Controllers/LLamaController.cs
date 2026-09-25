// LLamaController.cs

using Microsoft.AspNetCore.Mvc;
using LLMRemote.Services;

namespace LLMRemote.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LlamaController(LlamaService service) : ControllerBase{
    private readonly LlamaService _service = service;
    
    [HttpPost("start/{modelID:guid}")]
    public IActionResult Start(Guid modelID){
        _service.StartServer(modelID);
        return Ok();
    }
}
