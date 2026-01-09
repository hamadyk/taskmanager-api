namespace TaskManager.Api.Models;

public record TodoItem(
    Guid Id,
    string Title,
    bool IsCompleted
);