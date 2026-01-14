using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TodoBlobService>();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGet("/", (ILogger<Program> log) =>
{
    log.LogInformation("Hello from /");
    return "Hello World!";
});

app.MapGet("/api/todos", (ILogger<Program> log) =>
{
    log.LogInformation("GET /api/todos called");
    return new[] { "Todo1", "Todo2" };
});


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
// app.MapOpenApi();
//}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
