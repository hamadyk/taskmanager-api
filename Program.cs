using Azure.Core;
using Azure.Identity;
using Azure.Storage.Queues;
using TaskManager.Api.Services;
using TaskManager.Services;


var builder = WebApplication.CreateBuilder(args);

// ============================
// Application Insights
// ============================
builder.Services.AddApplicationInsightsTelemetry();

// ============================
// Azure Key Vault (PRO)
// ============================
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential()
    );
}

// ============================
// Services
// ============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TodoBlobService>();
// Blob client (singleton, cach

//builder.Services.AddSingleton(sp =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();

//    var accountName = config["Storage:AccountName"];
//    var queueName = config["Storage:QueueName"];

//    if (string.IsNullOrEmpty(accountName))
//        throw new InvalidOperationException("Storage:AccountName manquant");

//    if (string.IsNullOrEmpty(queueName))
//        throw new InvalidOperationException("Storage:QueueName manquant");

//    var queueUri = new Uri($"https://{accountName}.queue.core.windows.net/{queueName}");

//    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
//    {
//        // 🔒 On force Managed Identity uniquement
//        ExcludeEnvironmentCredential = true,
//        //ExcludeSharedTokenCacheCredential = true,
//        ExcludeVisualStudioCredential = true,
//        ExcludeVisualStudioCodeCredential = true,
//        ExcludeAzureCliCredential = true,
//        ExcludeAzurePowerShellCredential = true,
//        ExcludeInteractiveBrowserCredential = true
//    });

//    return new QueueClient(queueUri);
//});


builder.Services.AddSingleton<QueueClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IHostEnvironment>();

    var accountName = config["Storage:AccountName"];
    var queueName = config["Storage:QueueName"];

    var queueUri = new Uri($"https://{accountName}.queue.core.windows.net/{queueName}");

    TokenCredential credential;

    if (env.IsDevelopment())
    {
        // ✅ LOCAL : Azure CLI ou Visual Studio
        credential = new DefaultAzureCredential();
    }
    else
    {
        // 🔒 AZURE : Managed Identity UNIQUEMENT
        credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeEnvironmentCredential = true,
            ExcludeSharedTokenCacheCredential = true,
            ExcludeVisualStudioCredential = true,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeAzureCliCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeInteractiveBrowserCredential = true
        });
    }

    return new QueueClient(queueUri, credential);
});




builder.Services.AddHostedService<TodoWorker>();
//builder.Services.AddSingleton(sp =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();
//    var conn = config["StorageConnectionString"];
//    var containerName = config["Storage:ContainerName"];

//    return new BlobContainerClient(conn, containerName);
//});

var app = builder.Build();

// ============================
// Middleware
// ============================
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.MapControllers();

// ============================
// Health check simple
// ============================
app.MapGet("/", (ILogger<Program> log) =>
{
    log.LogInformation("Health check OK");
    return Results.Ok("API is running");
});

app.Run();
