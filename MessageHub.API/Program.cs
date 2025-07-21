using Application;
using Messaging.gRPC;
using System.Net.NetworkInformation;
using Messaging.EventHandler;
using SharedLayer.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers(options => { options.ReturnHttpNotAcceptable = true; })
    .AddNewtonsoftJson()
    .AddXmlDataContractSerializerFormatters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<HealthChecker>();

builder.Services.AddSingleton(new HealthChecker(
    new HttpClient(),
    "https://localhost:7159/api/module/health",
    MacAddressHelper.GenerateGuid()
));

builder.Services.AddSingleton<ClientManager>();
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