using System.Diagnostics;

namespace Aiursoft.AiurObserver;

public class ThrottledObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly TimeSpan _throttleTime;
    private readonly Stopwatch _stopwatch = new();

    public ThrottledObservable(IAsyncObservable<T> source, TimeSpan throttleTime)
    {
        _source = source;
        _throttleTime = throttleTime;
    }

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        _stopwatch.Start();
        var consumer = new Consumer<T>(async value =>
        {
            if (_stopwatch.Elapsed <= _throttleTime)
            {
                await Task.Delay(_throttleTime - _stopwatch.Elapsed);
            }
            await observer.Consume(value);
            _stopwatch.Restart();
        });
        return _source.Subscribe(consumer);
    }
}