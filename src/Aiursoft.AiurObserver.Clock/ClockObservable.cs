namespace Aiursoft.AiurObserver.Clock;

public class ClockObservable : AsyncObservable<DateTime>
{
    private readonly TimeSpan _interval;
    public ClockObservable(TimeSpan interval)
    {
        _interval = interval;
    }

    public async Task StartClock(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            await Task.Delay(_interval);
            await BroadcastAsync(DateTime.Now);
        }
    }
}