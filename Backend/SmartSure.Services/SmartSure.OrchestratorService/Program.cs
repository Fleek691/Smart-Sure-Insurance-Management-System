using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
// TODO: Add RabbitMQ connection and event bus setup here

var app = builder.Build();

app.MapControllers();

app.Run();
