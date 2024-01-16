namespace Aiursoft.AiurObserver.Features;

public class RepeatableObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly int _times;

    public RepeatableObservable(IAsyncObservable<T> source, int times)
    {
        _source = source;
        _times = times;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T>(async value =>
        {
            for (var i = 0; i < _times; i++)
            {
                await observer.Consume(value);
            }
        });
        return _source.Subscribe(consumer);
    }
}