using System.Diagnostics;

namespace Aiursoft.AiurObserver;

public class ThrottledObservable<T> : IAsyncObservable<T>
{
    private readonly IAsyncObservable<T> _source;
    private readonly TimeSpan _throttleTime;
    private readonly Stopwatch _stopwatch = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

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
            await _semaphore.WaitAsync();
            try 
            {
                if (_stopwatch.Elapsed <= _throttleTime)
                {
                    var waitTime = _throttleTime - _stopwatch.Elapsed;
                    await Task.Delay(waitTime);
                }
                
                await observer.Consume(value);
                _stopwatch.Restart();
            }
            finally
            {
                _semaphore.Release();
            }

        });
        return _source.Subscribe(consumer);
    }
}