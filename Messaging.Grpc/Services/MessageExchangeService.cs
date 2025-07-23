using Application.Contracts;
using Grpc.Core;
using Messaging.EventHandler;
using Messaging.Protos;
using Messaging.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Concurrent;

namespace Messaging.Grpc.Services;

public class MessageExchangeService : MessageChangeStream.MessageChangeStreamBase
{
    private readonly IGrpcHealthChecker _healthChecker;
    private readonly GrpcClientManager _clientManager;

    private readonly ConcurrentDictionary<string, IServerStreamWriter<MessageExchange>> _activeClients = new();

    public MessageExchangeService(IGrpcHealthChecker healthChecker, GrpcClientManager clientManager)
    {
        _healthChecker = healthChecker;
        _clientManager = clientManager;
    }


    public override async Task Communicate(IAsyncStreamReader<MessageExchange> requestStream,
        IServerStreamWriter<MessageExchange> responseStream, ServerCallContext context)
    {
        if (!await requestStream.MoveNext())
            return;

        var intro = requestStream.Current.Intro;
        if (intro == null)
            return;

        var clientId = intro.Id;

        _healthChecker.RegisterClient(clientId);
        _activeClients[clientId] = responseStream;

        try
        {
            await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _healthChecker.MarkClientActive(clientId);

                var healthState = _healthChecker.GetCurrentState();
                if (!healthState.IsEnabled)
                    break;

                if (message.PayloadCase == MessageExchange.PayloadOneofCase.Raw)
                {
                    var rawMsg = message.Raw;

                    var processed = new MessageExchange
                    {
                        Result = new ProcessedMessage
                        {
                            Id = rawMsg.Id,
                            Engine = "RegexEngine",
                            MessageLength = rawMsg.Message.Length,
                            IsValid = true,
                            RegexResults = { { "HasHello", rawMsg.Message.Contains("hello") } }
                        }
                    };

                    if (_activeClients.TryGetValue(clientId, out var clientStream))
                    {
                        await clientStream.WriteAsync(processed);
                    }
                }
            }
        }
        finally
        {
            _clientManager.CheckInactiveClients();
            _activeClients.TryRemove(clientId, out _);
        }
    }
}