using Grpc.Core;
using Messaging.Protos;
using Messaging.Queues;

namespace Messaging.Grpc.Services;

public class MessagingService : MessageStream.MessageStreamBase
{
    private readonly QueueSimulator _queueSimulator;

    public MessagingService(QueueSimulator queueSimulator)
    {
        _queueSimulator = queueSimulator;
    }

    public async override Task Communicate(IAsyncStreamReader<MessageEnvelope> requestStream,
        IServerStreamWriter<MessageEnvelope> responseStream, ServerCallContext context)
    {
        await foreach (var message in requestStream.ReadAllAsync())
        {
            if (message.Type == "Init")
            {
                Console.WriteLine($"[Init] Engine Connected :{message.Engine} - {message.Id}");
                continue;
            }

            if (message.Type == "Request")
            {
                var newMsg = await _queueSimulator.GetNextMessageAsync();

                var envelop = new MessageEnvelope
                {
                    Type = "Payload",
                    MessageId = newMsg.Id,
                    Message = newMsg.Content,
                    Engine = "RegexEngine",
                    Id = message.Id,
                };

                await responseStream.WriteAsync(envelop);
            }

            if (message.Type == "Response")
            {
                Console.WriteLine(
                    $"[Result] ID :{message.MessageId}, Length :{message.MessageLength}, Valid: {message.IsValid}");

                foreach (var kv in message.RegexResults)
                {
                    Console.WriteLine($" - {kv.Key} - {kv.Value}");
                }
            }
        }
    }
}