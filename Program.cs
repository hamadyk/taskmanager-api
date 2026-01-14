using Azure.Identity;
using Azure.Storage.Blobs;


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

// Blob client (singleton, cache)
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var conn = config["StorageConnectionString"];
    var containerName = config["Storage:ContainerName"];

    return new BlobContainerClient(conn, containerName);
});

var app = builder.Build();

// ============================
// Middleware
// ============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
