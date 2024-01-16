namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageStageLast<T> : AsyncObservable<T>, IConsumer<T>
{
    public bool IsStaged { get; private set; }

    public T? Stage { get; private set; }
    
    public async Task Consume(T value)
    {
        if (!IsStaged)
        {
            IsStaged = true;
        }
        Stage = value;
        await BroadcastAsync(value);
    }
}