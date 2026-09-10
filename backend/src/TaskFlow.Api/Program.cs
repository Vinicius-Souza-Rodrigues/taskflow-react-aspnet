using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Infra;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<TaskFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost", "http://localhost:80")
            .AllowAnyMethod()
            .AllowAnyHeader()));

var app = builder.Build();

// Applies pending migrations on startup — from the Fase 3 (Compose) task list, so the
// database is always in sync with the deployed image without a manual `dotnet ef` step.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors(FrontendCorsPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

// Exposes Program for WebApplicationFactory<Program> in TaskFlow.Api.Tests.
public partial class Program { }
