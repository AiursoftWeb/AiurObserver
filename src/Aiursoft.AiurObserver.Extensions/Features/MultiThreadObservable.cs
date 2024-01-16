namespace Aiursoft.AiurObserver.Features;

public class MultiThreadObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    public MultiThreadObservable(IAsyncObservable<T> source)
    {
        _source = source;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T>(async value =>
        {
            await Task.Factory.StartNew(() => observer.Consume(value), TaskCreationOptions.LongRunning);
        });
        return _source.Subscribe(consumer);
    }
}