using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Grpc.Net.Client;
using Messaging.Grpc.Services;
using Messaging.Protos;
using Microsoft.Extensions.Hosting;
using SharedLayer.Common;
using Google.Protobuf;
namespace Processing.Workers;

public class WorkerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7199");
        var client = new MessageChangeStream.MessageChangeStreamClient(channel);

       using var call = client.Communicate(cancellationToken: stoppingToken);

        var intro = new IntroductionMessage
        {
            Id = GetMachineBasedGuid(),
            Type = "RegexEngine"
        };

        await call.RequestStream.WriteAsync(new MessageExchange
        {
            Intro = intro
        });

        Console.WriteLine("Sending message...");
        
        // listen to server messages :
    }


    private string GetMachineBasedGuid()
    {
        var mac = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(_ => _.OperationalStatus == OperationalStatus.Up)?
            .GetPhysicalAddress().ToString();

        return string.IsNullOrEmpty(mac)
            ? Guid.NewGuid().ToString()
            : GuidUtility.Create(GuidUtility.UrlNamespace, mac).ToString();
    }
}