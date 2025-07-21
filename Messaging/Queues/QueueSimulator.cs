using SharedLayer.Contracts;

namespace Messaging.Queues;

public class QueueSimulator
{
    private static Random _random = new();

    public async Task<Message> GetNextMessageAsync()
    {
        await Task.Delay(200);
        return new Message()
        {
            Id = _random.Next(1, 9999),
            Sender = _random.Next(2) == 0 ? "Legal" : "User",
            Content = "Test Content" + Guid.NewGuid().ToString("N"),
        };
    }
}