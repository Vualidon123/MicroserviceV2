using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

// Register the consumer and background service
builder.Services.AddSingleton<IConsumer, Consumer>();
builder.Services.AddHostedService<ConsumerBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline if needed

app.Run(); 