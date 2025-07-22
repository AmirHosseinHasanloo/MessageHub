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
                var dto = await _queueSimulator.GetNextMessageAsync();

                if (dto is not null)
                {
                    var envelope = new MessageEnvelope
                    {
                        Type = "Payload",
                        Id = message.Id, // همون آیدی کلاینت فرستنده
                        Engine = "RegexEngine",
                        MessageId = dto.Id,
                        Message = dto.Message // ✅ اسم درست خاصیت Content هست، نه Message
                    };

                    await responseStream.WriteAsync(envelope);
                }
                else
                {
                    Console.WriteLine("📭 صف پیام خالی بود، پیامی ارسال نشد.");
                }
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