using Messaging.Protos;
using SharedLayer.Contracts;
using SharedLayer.Contracts.MessageDTOs;
using System.Collections.Concurrent;
namespace Messaging.Queues;


public class QueueSimulator
{
    private readonly ConcurrentQueue<RawMessageDto> _messages = new();

    public void DoEnqueue(RawMessageDto message)
    {
        if (message is not null)
            _messages.Enqueue(message);
    }

    public Task<RawMessageDto> GetNextMessageAsync()
    {
        if (_messages.TryDequeue(out var message))
            return Task.FromResult(message);

        return Task.FromResult<RawMessageDto>(null);
    }
}