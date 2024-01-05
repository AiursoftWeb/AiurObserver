namespace Aiursoft.AiurObserver;

public class MessageCounter<T> : AsyncObservable<int>, IConsumer<T>
{
    public int Count { get; private set; }
    
    public async Task Consume(T value)
    {
        Count++;
        await BroadcastAsync(Count);
    }
}