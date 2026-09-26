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

    [HttpGet("{model_ID}")]
    public IActionResult Get(Guid model_ID){
        return Ok(_service.Get(model_ID));
    }

    [HttpGet("all")]
    public IActionResult GetAll([FromQuery] int? page, int? count, LLMModelsService.OrderBy? orderBy){
        if(page != null && page < 1){
            ModelState.AddModelError(nameof(page), "Must be 1 or more.");
        }else if (count != null && count < 1){
            ModelState.AddModelError(nameof(count), "Must be 1 or more.");
        }else if (orderBy != null && !Enum.IsDefined(orderBy.Value)){
            ModelState.AddModelError(nameof(orderBy), "Not a valid value");
        }
        if(!ModelState.IsValid) return ValidationProblem(ModelState);
        return Ok(_service.GetAll(page:page ??= 1, count: count??= 10, orderBy: orderBy ??=LLMModelsService.OrderBy.Name));
    }

    [HttpPut("{model_ID}")]
    public IActionResult Patch(Guid model_ID, [FromBody] LLMModelRequest request){
        return Ok(_service.Update(model_ID, request));
    }

    [HttpDelete("{model_ID}")]
    public IActionResult Delete(Guid model_ID){
        _service.Delete(model_ID);
        return NoContent();
    }
}
