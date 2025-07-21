using System.ComponentModel;
using System.Text.RegularExpressions;
using Grpc.Net.Client;
using Messaging.Grpc.Services;
using Messaging.Protos;
using Microsoft.Extensions.Hosting;
using SharedLayer.Contracts;

namespace Processing.Workers;

public class ProcessingWorker : BackgroundService
{
    private readonly string _id = Guid.NewGuid().ToString();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = GrpcChannel.ForAddress("https://localhost:7199");
        var client = new MessageStream.MessageStreamClient(channel);

        using var call = client.Communicate();

        //Initial Message
        await call.RequestStream.WriteAsync(new MessageEnvelope
        {
            Type = "Init",
            Id = _id,
            Engine = "Regex Engine",
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            // request new message
            await call.RequestStream.WriteAsync(new MessageEnvelope
            {
                Type = "Request",
                Id = _id
            });

            // Read response
            if (await call.ResponseStream.MoveNext(stoppingToken))
            {
                var incoming = call.ResponseStream.Current;
                var result = AnalyzeMessage(incoming);

                await call.RequestStream.WriteAsync(result);
            }

            await Task.Delay(200, stoppingToken);
        }

        await call.RequestStream.CompleteAsync();
    }

    private MessageEnvelope AnalyzeMessage(MessageEnvelope message)
    {
        var regexRules = new Dictionary<string, string>
        {
            { "HasHello", "hello" },
            { "HasNumber", "\\d+" },
        };

        var result = new MessageEnvelope()
        {
            Type = "Response",
            Id = _id,
            MessageId = message.MessageId,
            Message = message.Message,
            Engine = message.Engine,
            MessageLength = message.Message.Length,
            IsValid = true,
            RegexResults = { }
        };
        foreach (var rule in regexRules)
        {
            result.RegexResults.Add(rule.Key, Regex.IsMatch(message.Message, rule.Value));
        }

        return result;
    }
}