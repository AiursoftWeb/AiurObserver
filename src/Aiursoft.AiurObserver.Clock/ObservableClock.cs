namespace Aiursoft.AiurObserver.Clock;

/// <summary>
/// Represents an observable clock that emits the current date and time at a specified interval.
/// </summary>
public class ObservableClock : AsyncObservable<DateTime>
{
    private readonly TimeSpan _interval;

    /// <summary>
    /// Represents an observable clock that raises an event at specified intervals. </summary> <param name="interval">The time span interval at which the event should be raised.</param>
    /// /
    public ObservableClock(TimeSpan interval)
    {
        _interval = interval;
    }

    /// <summary>
    /// Starts the clock and continuously broadcasts the current date and time until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to request cancellation of the clock.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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