namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageStageFirst<T> : AsyncObservable<T>, IConsumer<T>
{
    public bool IsStaged { get; private set; }

    public T? Stage { get; private set; }
    public async Task Consume(T value)
    {
        if (!IsStaged)
        {
            Stage = value;
            IsStaged = true;
            await BroadcastAsync(value);
        }
    }
}