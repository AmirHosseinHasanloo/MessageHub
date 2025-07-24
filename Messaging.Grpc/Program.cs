using Application.Contracts;
using Infrastructure;
using Messaging.EventHandler;
using Messaging.Grpc.Services;
using Messaging.Protos;
using Messaging.Queues;
using Messaging.Services;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<QueueSimulator>();
builder.Services.AddHttpClient<HealthChecker>();
builder.Services.AddSingleton(Channel.CreateUnbounded<MessageExchange>());

builder.Services.AddSingleton<GrpcClientManager>();
builder.Services.AddScoped<IGrpcHealthChecker>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var logger = sp.GetRequiredService<ILogger<GrpcHealthChecker>>();
    var clientManager = sp.GetRequiredService<GrpcClientManager>();

    var httpClient = httpClientFactory.CreateClient();
    var healthUrl = "https://localhost:7159/api/HealthCheck/health";
    var id = Environment.MachineName;

    return new GrpcHealthChecker(httpClient, healthUrl, id, logger, clientManager);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<MessageExchangeService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();