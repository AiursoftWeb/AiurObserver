namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageRadio<T> : AsyncObservable<T>, IConsumer<T>
{
    public Task Consume(T value)
    {
        return BroadcastAsync(value);
    }
}