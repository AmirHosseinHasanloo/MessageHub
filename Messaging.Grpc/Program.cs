using Application.Contracts;
using Infrastructure;
using Messaging.EventHandler;
using Messaging.Grpc.Services;
using Messaging.Queues;
using Messaging.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<QueueSimulator>();
builder.Services.AddHttpClient<HealthChecker>();
builder.Services.AddSingleton<GrpcClientManager>();
builder.Services.AddScoped<IGrpcHealthChecker, GrpcHealthChecker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<MessagingService>();
app.MapGrpcService<MessageExchangeService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();