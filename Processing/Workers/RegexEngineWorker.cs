using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Grpc.Net.Client;
using Messaging.Grpc.Services;
using Messaging.Protos;
using Microsoft.Extensions.Hosting;
using SharedLayer.Common;
using Google.Protobuf;
using SharedLayer.Contracts;
using Grpc.Core;
namespace Processing.Workers;

public class RegexEngineWorker : BackgroundService
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

        Console.WriteLine("Sent Introduction message");

        // ???? ????? ?? ???? Raw ?????????:
        await call.RequestStream.WriteAsync(new MessageExchange
        {
            Raw = new RawMessage
            {
                Id = 1,
                Sender = "RegexEngineWorker",
                Message = "Hello 123 from RegexEngine!"
            }
        });

        Console.WriteLine("Sent RawMessage");

        // ?????? ???????:
        await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
        {
            if (response.PayloadCase == MessageExchange.PayloadOneofCase.Result)
            {
                Console.WriteLine($" Received ProcessedMessage with ID {response.Result.Id}");
                Console.WriteLine($" Valid: {response.Result.IsValid}");
                Console.WriteLine($" Length: {response.Result.MessageLength}");

                foreach (var kv in response.Result.RegexResults)
                {
                    Console.WriteLine($"    Regex '{kv.Key}': {kv.Value}");
                }
            }
        }
    }
    private string GetMachineBasedGuid()
    {
        var mac = NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(i => i.OperationalStatus == OperationalStatus.Up)?
            .GetPhysicalAddress()
            .ToString();

        return string.IsNullOrEmpty(mac)
            ? Guid.NewGuid().ToString()
            : GuidUtility.Create(GuidUtility.UrlNamespace, mac).ToString();
    }
}