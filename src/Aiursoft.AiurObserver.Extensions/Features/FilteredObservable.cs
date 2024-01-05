namespace Aiursoft.AiurObserver;

public class FilteredObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly Func<T, bool> _predicate;

    public FilteredObservable(IAsyncObservable<T> source, Func<T, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T>(async value =>
        {
            if (_predicate(value))
            {
                await observer.Consume(value);
            }
        });
        return _source.Subscribe(consumer);
    }
}