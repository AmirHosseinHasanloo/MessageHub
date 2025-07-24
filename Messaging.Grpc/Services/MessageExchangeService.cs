using Application.Contracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Messaging.EventHandler;
using Messaging.Protos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Messaging.Grpc.Services;

public class MessageExchangeService : MessageChangeStream.MessageChangeStreamBase
{
    private readonly IGrpcHealthChecker _healthChecker;
    private readonly GrpcClientManager _clientManager;
    private readonly Channel<MessageExchange> _messageChannel;
    private readonly ILogger<MessageExchangeService> _logger;

    private readonly ConcurrentDictionary<string, IServerStreamWriter<MessageExchange>> _activeClients = new();

    public MessageExchangeService(IGrpcHealthChecker healthChecker,
        GrpcClientManager clientManager,
        Channel<MessageExchange> messageChannel,
        ILogger<MessageExchangeService> logger)
    {
        _healthChecker = healthChecker;
        _clientManager = clientManager;
        _messageChannel = messageChannel;
        _logger = logger;
    }

    public override async Task<Empty> SendRawMessage(RawMessage request, ServerCallContext context)
    {
        var exchangeMessage = new MessageExchange()
        {
            Raw = request
        };

        await _messageChannel.Writer.WriteAsync(exchangeMessage);

        // this is google protobuf.empty
        return new Empty();
    }

    public override async Task Communicate(
        IAsyncStreamReader<MessageExchange> requestStream,
        IServerStreamWriter<MessageExchange> responseStream,
        ServerCallContext context)
    {
        if (!await requestStream.MoveNext(context.CancellationToken))
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

                switch (message.PayloadCase)
                {
                    case MessageExchange.PayloadOneofCase.Raw:
                        await HandleRawMessage(clientId, message.Raw);
                        break;

                    case MessageExchange.PayloadOneofCase.Result:
                        // اگه در آینده خواستی سمت کلاینت ProcessedMessage بفرسته
                        break;

                    default:
                        break;
                }
            }
        }
        finally
        {
            _clientManager.CheckInactiveClients();
            _activeClients.TryRemove(clientId, out _);
        }
    }

    private async Task HandleRawMessage(string clientId, RawMessage raw)
    {
        var result = new MessageExchange
        {
            Result = new ProcessedMessage
            {
                Id = raw.Id,
                Engine = "RegexEngine",
                MessageLength = raw.Message.Length,
                IsValid = true,
                RegexResults = { { "HasHello", raw.Message.Contains("hello") } }
            }
        };

        if (_activeClients.TryGetValue(clientId, out var stream))
        {
            await stream.WriteAsync(result);
        }
    }
}