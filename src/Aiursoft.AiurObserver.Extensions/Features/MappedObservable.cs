namespace Aiursoft.AiurObserver.Features;

public class MappedObservable<T1, T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T1> _source;
    private readonly Func<T1, T> _mapper;

    public MappedObservable(IAsyncObservable<T1> source, Func<T1, T> mapper)
    {
        _source = source;
        _mapper = mapper;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        return _source.Subscribe(value => observer.Consume(_mapper(value)));
    }
}