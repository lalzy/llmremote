// LLMModelController.cs

using Microsoft.AspNetCore.Mvc;
using LLMRemote.Services;
using LLMRemote.Models;

namespace LLMRemote.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LLMModelController(LLMModelsService service) : ControllerBase{
    private readonly LLMModelsService _service = service;

    [HttpPost("add")]
    public IActionResult Add([FromBody] LLMModelRequest request){
        return Ok(_service.Add(request));
    }
}
