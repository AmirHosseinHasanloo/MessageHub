using Grpc.Core;
using Messaging.Protos;

namespace Messaging.Grpc.Services;

public class MessageExchangeService : MessageChangeStream.MessageChangeStreamBase
{
    public async override Task Communicate(IAsyncStreamReader<MessageExchange> requestStream, IServerStreamWriter<MessageExchange> responseStream, ServerCallContext context)
    {
        await foreach (var message in requestStream.ReadAllAsync(context.CancellationToken))
        {
            if (message.PayloadCase == MessageExchange.PayloadOneofCase.Intro)
            {
             var intro = message.Intro;
             Console.WriteLine($"new Processor exists now : ID={intro.Id} - Type={intro.Type}");
             
             // : Save Client info in database
            }
        }
    }
}