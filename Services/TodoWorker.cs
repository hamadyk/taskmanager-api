using Azure.Storage.Queues;
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
            ILogger<TodoWorker> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    var messages = await _queue.ReceiveMessagesAsync(maxMessages: 1, visibilityTimeout: TimeSpan.FromSeconds(30));


                    if (messages.Value.Length == 0)
                    {
                        // Queue vide → on attend avant de re-tester
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }
                    foreach (var msg in messages.Value)
                    {
                        _logger.LogInformation("Processing todo: {Todo}", msg.MessageText);
                        var todo = JsonSerializer.Deserialize<List<TodoItem>>(msg.MessageText);

                        await _blob.SaveTodosAsync(todo);
                        // simulate work
                        await Task.Delay(2000, stoppingToken);

                        await _queue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message.ToString()); ;
                }
            }
        }
    }

}
