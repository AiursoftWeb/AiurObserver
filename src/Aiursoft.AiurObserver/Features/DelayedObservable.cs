namespace Aiursoft.AiurObserver;

public class DelayedObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly TimeSpan _delay;
    public DelayedObservable(IAsyncObservable<T> source, TimeSpan delay)
    {
        _source = source;
        _delay = delay;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T>(async value =>
        {
            await Task.Delay(_delay);
            await observer.Consume(value);
        });
        return _source.Subscribe(consumer);
    }
}