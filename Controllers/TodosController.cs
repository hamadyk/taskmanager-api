
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<TodosController> _logger;

    public TodosController(
        BlobContainerClient container,
        ILogger<TodosController> logger)
    {
        _container = container;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("GET /api/todos called");
        return Ok(new[] { "Todo 1", "Todo 2" });
    }

    //[HttpGet]
    //public async Task<IActionResult> Get()
    //{
    //    var todos = await _service.GetTodosAsync();
    //    return Ok(todos);
    //}

    //[HttpPost]
    //public async Task<IActionResult> Create(TodoItem todo)
    //{
    //    var todos = await _service.GetTodosAsync();
    //    var item = todo with { Id = Guid.NewGuid() };
    //    todos.Add(item);
    //    await _service.SaveTodosAsync(todos);
    //    return Ok(item);
    //}
}
