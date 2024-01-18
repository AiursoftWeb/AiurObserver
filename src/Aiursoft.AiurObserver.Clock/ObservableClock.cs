namespace Aiursoft.AiurObserver.Clock;

public class ObservableClock : AsyncObservable<DateTime>
{
    private readonly TimeSpan _interval;
    public ObservableClock(TimeSpan interval)
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
            await Task.Delay(_interval, cancellationToken);
            await BroadcastAsync(DateTime.Now);
        }
    }
}