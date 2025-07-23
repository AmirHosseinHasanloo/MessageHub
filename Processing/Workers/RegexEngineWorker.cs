using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Grpc.Net.Client;
using Messaging.Protos;
using Microsoft.Extensions.Hosting;
using SharedLayer.Common;
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


        Console.WriteLine("Sent RawMessage");

        await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
        {
            if (response.PayloadCase == MessageExchange.PayloadOneofCase.Raw)
            {
                var raw = response.Raw;
                Console.WriteLine($"[Recived] ID:{raw.Id}, Msg: {raw.Message}");

                var result = new ProcessedMessage()
                {
                    Id = raw.Id,
                    Engine = "Regex Engine",
                    IsValid = raw.Message.Length > 0,
                    MessageLength = raw.Message.Length,
                };

                var matches = Regex.Matches(raw.Message, @"\d+");
                bool hasNumbers = matches.Count > 0;

                result.RegexResults.Add("Numbers", hasNumbers);

                await call.RequestStream.WriteAsync(new MessageExchange
                {
                    Result = result
                });

                Console.WriteLine($"[sent] Processed Message ID: {result.Id}");
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