namespace Aiursoft.AiurObserver.Features;

public class SampleObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly int _every;
    private int _counter;

    public SampleObservable(IAsyncObservable<T> source, int every)
    {
        if (every < 1)
        {
            throw new ArgumentException("Every must be greater than 0.");
        }
        _source = source;
        _every = every;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        _counter = _every;
        var consumer = new Consumer<T>(async value =>
        {
            if (_counter == 1)
            {
                await observer.Consume(value);
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