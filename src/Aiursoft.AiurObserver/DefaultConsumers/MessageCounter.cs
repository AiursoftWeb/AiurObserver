namespace Aiursoft.AiurObserver;

public class MessageCounter<T>
{
    public MessageCounter(IAsyncObservable<T> source)
    {
        source.Subscribe(_ =>
        {
            Count++;
            return Task.CompletedTask;
        });
    }

    public int Count { get; private set; }
}