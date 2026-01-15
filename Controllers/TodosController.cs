using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{

    private readonly ILogger<TodosController> _logger;
    private readonly TodoBlobService _service;
    public TodosController(
        ILogger<TodosController> logger, TodoBlobService service)
    {
        _logger = logger;
        _service = service;
    }

    //[HttpGet]
    //public IActionResult Get()
    //{
    //    _logger.LogInformation("GET /api/todos called");
    //    return Ok(new[] { "Todo 1", "Todo 2" });
    //}

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        _logger.LogInformation("GET /api/todos called");
        var todos = await _service.GetTodosAsync();
        return Ok(todos);
    }

    //[HttpPost]
    //public async Task<IActionResult> Create(TodoItem todo)
    //{
    //    _logger.LogInformation("GET /api/todos set");

    //    var todos = await _service.GetTodosAsync();
    //    var item = todo with { Id = Guid.NewGuid() };
    //    todos.Add(item);
    //    await _service.SaveTodosAsync(todos);
    //    return Ok(item);
    //}

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoItem todo, QueueClient queueClient)
    {
        var todos = new List<TodoItem>();
        var item = todo with { Id = Guid.NewGuid() };
        todos.Add(item);
        var message = JsonSerializer.Serialize(todo);
        await queueClient.SendMessageAsync(message);
        return Accepted("Todo queued");
    }

}
