// LLamaController.cs

using Microsoft.AspNetCore.Mvc;
using LLMRemote.Services;
using LLMRemote.Models;

namespace LLMRemote.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LlamaController(LlamaService service) : ControllerBase{
    private readonly LlamaService _service = service;
    
    [HttpPost("start")]
    public IActionResult Start([FromBody] LLMModel model){
        _service.StartServer(model);
        return Ok();
    }

    [HttpPost("stop")]
    public IActionResult Stop(){
        _service.StopServer();
        return Ok();
    }
}
