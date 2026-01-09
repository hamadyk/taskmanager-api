using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly TodoBlobService _service;
    public TodosController(TodoBlobService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var todos = await _service.GetTodosAsync();
        return Ok(todos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TodoItem todo)
    {
        var todos = await _service.GetTodosAsync();
        var item = todo with { Id = Guid.NewGuid() };
        todos.Add(item);
        await _service.SaveTodosAsync(todos);
        return Ok(item);
    }
}
