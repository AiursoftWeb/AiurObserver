namespace Aiursoft.AiurObserver.Features;

public class LockedObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LockedObservable(IAsyncObservable<T> source)
    {
        _source = source;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T>(async value =>
        {
            await _semaphore.WaitAsync();
            try
            {
                await observer.Consume(value);
            }
            finally
            {
                _semaphore.Release();
            }
        });
        return _source.Subscribe(consumer);
    }
}