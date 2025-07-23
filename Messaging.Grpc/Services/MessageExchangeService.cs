using Grpc.Core;
using Messaging.EventHandler;
using Messaging.Protos;
using Messaging.Services;
using System.Collections.Concurrent;

namespace Messaging.Grpc.Services;

public class MessageExchangeService : MessageChangeStream.MessageChangeStreamBase
{
    private readonly HealthChecker _healthChecker;
    private readonly ClientManager _clientManager;

    // ????? ?????????? ???? ?? ????? ???? ????? ??????
    private readonly ConcurrentDictionary<string, IServerStreamWriter<MessageExchange>> _activeClients = new();

    public MessageExchangeService(HealthChecker healthChecker, ClientManager clientManager)
    {
        _healthChecker = healthChecker;
        _clientManager = clientManager;
    }

    public override async Task Communicate(IAsyncStreamReader<MessageExchange> requestStream,
        IServerStreamWriter<MessageExchange> responseStream, ServerCallContext context)
    {
        // ????? ?????? ???? ????? (Intro)
        if (!await requestStream.MoveNext())
            return;

        var intro = requestStream.Current.Intro;
        if (intro == null)
        {
            // ???? ????? ?????? ???
            return;
        }

        var clientId = intro.Id;

        // ??? ?????? ????
        _clientManager.RegisterClient(clientId);

        // ????? ?????? ???? ????? ???????
        _activeClients[clientId] = responseStream;

        try
        {
            await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
            {
                _clientManager.MarkClientActive(clientId);

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
                else if (message.PayloadCase == MessageExchange.PayloadOneofCase.Result)
                {
                    var result = message.Result;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[Result] ID: {result.Id}, Length: {result.MessageLength}, IsValid: {result.IsValid}");

                    foreach (var kv in result.RegexResults)
                    {
                        Console.WriteLine($" - {kv.Key} : {kv.Value}");
                    }

                    Console.ResetColor();
                }
            }

        }
        finally
        {
            // ??? ???? ?????? ????? ??? ??????
            _clientManager.CheckInActiveClients();
            _activeClients.TryRemove(clientId, out _);
        }
    }
}