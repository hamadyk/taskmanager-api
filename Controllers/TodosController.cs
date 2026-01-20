using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using System.Text;
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
    private readonly QueueClient _queue;
    public TodosController(
        ILogger<TodosController> logger, TodoBlobService service, QueueClient queue)
    {
        _logger = logger;
        _service = service;
        _queue = queue;
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
        var todos = await _service.GetTodosAsync(); ;
        var item = todo with { Id = Guid.NewGuid() };
        todos.Add(item);
        var message = JsonSerializer.Serialize(todos);
        _logger.LogInformation(
    "SEND → Account: {Account}, Queue: {Queue}",
    _queue.AccountName,
    _queue.Name);
        var messages = Convert.ToBase64String(
             Encoding.UTF8.GetBytes(message));
        await _queue.SendMessageAsync(messages);
        return Accepted("Todo queued");
    }

}
