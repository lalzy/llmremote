// NvidiaMonitorController.cs

using Microsoft.AspNetCore.Mvc;
using LLMRemote.Services;
using LLMRemote.Models;

namespace LLMRemote.Controllers;

[ApiController]
[Route("api/nvidia")]
public class NvidiaMonitorController (NvidiaMonitorService service) : ControllerBase{
    private readonly NvidiaMonitorService _service = service;

    [HttpGet("usage")]
    public IActionResult Usage(){
        return Ok(_service.GetUsage());
    }
}
