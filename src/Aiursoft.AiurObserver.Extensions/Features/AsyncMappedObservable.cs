namespace Aiursoft.AiurObserver.Features;

public class AsyncMappedObservable<T1, T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T1> _source;
    private readonly Func<T1, Task<T>> _mapper;

    public AsyncMappedObservable(IAsyncObservable<T1> source, Func<T1, Task<T>> mapper)
    {
        _source = source;
        _mapper = mapper;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        return _source.Subscribe(async value => await observer.Consume(await _mapper(value)));
    }
}