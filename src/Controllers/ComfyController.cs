// ComfyController.cs

using Microsoft.AspNetCore.Mvc;
using LLMRemote.Services;
using LLMRemote.Models;

namespace LLMRemote.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComfyController(ComfyService service) : ControllerBase{
    private readonly ComfyService _service = service;
    
    [HttpPost("start")]
    public IActionResult Start(){
        _service.StartServer();
        return Ok();
    }

    [HttpPost("stop")]
    public IActionResult Stop(){
        _service.StopServer();
        return Ok();
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health(){
        return Ok(await _service.RunningP());
    }
}
