using Application.Contracts;
using Infrastructure;
using Messaging.EventHandler;
using Messaging.Grpc.Services;
using Messaging.Services;
using SharedLayer.Common;
using System.Net.NetworkInformation;
using Messaging.Protos; // یا namespace موجود در فایل .proto


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers(options => { options.ReturnHttpNotAcceptable = true; })
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<HealthChecker>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7159");
});

builder.Services.AddHttpClient<HealthChecker>();
builder.Services.AddSingleton<HealthChecker>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<HealthChecker>>();
    var clientManager = sp.GetRequiredService<ClientManager>();
    var id = MacAddressHelper.GenerateStableId();
    var healthUrl = "https://localhost:7159/api/module/health";
    return new HealthChecker(httpClient, healthUrl, id, logger, clientManager);
});



builder.Services.AddLogging();

builder.Services.AddSingleton<ClientManager>();
builder.Services.AddHostedService<ClientCleanupService>();

builder.Services.AddGrpcClient<MessageChangeStream.MessageChangeStreamClient>(options =>
{
    options.Address = new Uri("https://localhost:7199");
});


#region DI Container


builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

#endregion

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();