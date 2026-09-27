// ShutdownTimerController.cs

using Microsoft.AspNetCore.Mvc;
using LLMRemote.Services;
using LLMRemote.Models;

namespace LLMRemote.Controllers;

[ApiController]
[Route("api/timer")]
public class ShutdownTimerController(ShutdownTimerService service) : ControllerBase{
    private readonly ShutdownTimerService _service = service;

    [HttpPost("set")]
    public IActionResult Set([FromQuery] int duration){
        _service.Set(duration);
        return NoContent();
    }

    [HttpGet("shutdowntime")]
    public IActionResult ShutdownTime(){
        return Ok(_service.ShutdownAt);
    }

    [HttpPost("cancel")]
    public IActionResult Cancel(){
        _service.Cancel();
        return NoContent();
    }
}
