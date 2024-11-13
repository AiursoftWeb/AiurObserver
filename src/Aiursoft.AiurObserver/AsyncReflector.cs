namespace Aiursoft.AiurObserver;

public class AsyncReflector<T> : AsyncObservable<T>, IConsumer<T>
{
    public Task Consume(T value) => BroadcastAsync(value);
    
}