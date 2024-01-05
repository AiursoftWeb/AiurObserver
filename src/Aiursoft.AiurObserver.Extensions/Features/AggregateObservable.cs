namespace Aiursoft.AiurObserver;

public class AggregateObservable<T> : IAsyncObservable<T[]>
{
    private readonly IAsyncObservable<T> _source;
    private readonly int _every;
    private readonly List<T> _buffer = new();
    private int _counter;

    public AggregateObservable(IAsyncObservable<T> source, int every)
    {
        if (every < 1)
        {
            throw new ArgumentException("Every must be greater than 0.");
        }
        _source = source;
        _every = every;
    }

    public ISubscription Subscribe(IConsumer<T[]> observer)
    {
        _counter = _every;
        var consumer = new Consumer<T>(async value =>
        {
            _buffer.Add(value);
            if (_counter == 1)
            {
                await observer.Consume(_buffer.ToArray());
                _buffer.Clear();
                _counter = _every;
            }
            else
            {
                _counter--;
            }
        });
        return _source.Subscribe(consumer);
    }
}