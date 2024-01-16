namespace Aiursoft.AiurObserver.Features;

public class SampleDoObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly Func<T, Task> _action;
    private readonly int _every;
    private int _counter;

    public SampleDoObservable(IAsyncObservable<T> source, int every, Func<T, Task> action)
    {
        if (every < 1)
        {
            throw new ArgumentException("Every must be greater than 0.");
        }
        _source = source;
        _every = every;
        _action = action;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        _counter = _every;
        var consumer = new Consumer<T>(async value =>
        {
            if (_counter == 1)
            {
                await _action(value);
                _counter = _every;
            }
            else
            {
                _counter--;
            }
            
            await observer.Consume(value);
        });
        return _source.Subscribe(consumer);
    }
}