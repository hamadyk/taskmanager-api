using Azure.Storage.Queues;
using System.Text;
using System.Text.Json;
using TaskManager.Api.Models;
using TaskManager.Api.Services;
namespace TaskManager.Services
{
    public class TodoWorker : BackgroundService
    {
        private readonly QueueClient _queue;
        private readonly ILogger<TodoWorker> _logger;
        private readonly TodoBlobService _blob;

        public TodoWorker(
            QueueClient queue,
            ILogger<TodoWorker> logger, TodoBlobService blob)
        {
            _queue = queue;
            _logger = logger;
            _blob = blob;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation(
    "RECEIVE → Account: {Account}, Queue: {Queue}",
    _queue.AccountName,
    _queue.Name);
                    var messages = await _queue.ReceiveMessagesAsync(
                        maxMessages: 1,
                        visibilityTimeout: TimeSpan.FromSeconds(30),
                        cancellationToken: stoppingToken);

                    if (messages.Value.Length == 0)
                    {
                        // Queue vide → on attend avant de re-tester
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }

                    foreach (var msg in messages.Value)
                    {

                        _logger.LogInformation("Processing todo: {Todo}", msg.MessageText);
                        byte[] byteArray = Convert.FromBase64String(msg.MessageText);
                        string jsonBack = Encoding.UTF8.GetString(byteArray);
                        var todo = JsonSerializer.Deserialize<List<TodoItem>>(jsonBack);

                        await _blob.SaveTodosAsync(todo);
                        // Simulation de traitement
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                        await _queue.DeleteMessageAsync(
                            msg.MessageId,
                            msg.PopReceipt,
                            stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Arrêt propre du service (normal lors du shutdown Azure)
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing queue message");
                    // Backoff en cas d’erreur pour éviter une boucle agressive
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

    }

}
