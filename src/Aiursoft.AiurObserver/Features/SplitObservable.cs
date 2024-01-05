using Aiursoft.AiurObserver;

public class SplitObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T[]> _source;

    public SplitObservable(IAsyncObservable<T[]> source)
    {
        _source = source;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T[]>(async values =>
        {
            foreach (var value in values)
            {
                await observer.Consume(value);
            }
        });
        return _source.Subscribe(consumer);
    }
}