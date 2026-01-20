using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using System.Text.Json;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public class TodoBlobService
{
    private readonly BlobContainerClient _container;

    public TodoBlobService(IConfiguration config)
    {
        var keyVaultUrl = config["KeyVault:Url"];
        var secretName = "StorageConnectionString";

        var secretClient = new SecretClient(
            new Uri(keyVaultUrl),
            new DefaultAzureCredential()
        );

        var secret = secretClient.GetSecret(secretName);

        _container = new BlobContainerClient(
            secret.Value.Value,
            config["Storage:ContainerName"]
        );

        _container.CreateIfNotExists();
    }

    public async Task<List<TodoItem>> GetTodosAsync()
    {
        var blob = _container.GetBlobClient("todos.json");

        if (!await blob.ExistsAsync())
            return [];

        var content = await blob.DownloadContentAsync();
        return JsonSerializer.Deserialize<List<TodoItem>>(content.Value.Content.ToString())
               ?? [];
    }
    public async Task SaveTodosAsync(List<TodoItem> todos)
    {
        var blob = _container.GetBlobClient("todos.json");
        var json = JsonSerializer.Serialize(todos);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        await blob.UploadAsync(stream, overwrite: true);
    }
}
