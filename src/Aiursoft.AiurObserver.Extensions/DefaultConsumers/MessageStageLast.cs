namespace Aiursoft.AiurObserver.DefaultConsumers;

/// <summary>
/// Represents a consumer that stages the last consumed message and supports asynchronous waiting for events.
/// This class allows for broadcasting messages to observers, caching the most recent message,
/// and providing mechanisms to wait for the next event or retrieve the staged event.
/// </summary>
/// <typeparam name="T">The type of the message being consumed and broadcasted.</typeparam>
public class MessageStageLast<T> : AsyncObservable<T>, IConsumer<T>
{
    private TaskCompletionSource<T>? _tcs;
    private readonly object _lock = new();

    /// <summary>
    /// Indicates whether a message has been staged.
    /// If <c>true</c>, a message is stored in the <see cref="Stage"/> property and can be accessed.
    /// </summary>
    public bool IsStaged { get; private set; }

    /// <summary>
    /// Stores the last consumed message.
    /// This value is updated every time a message is consumed and can be retrieved using <see cref="WaitOneEvent"/>.
    /// </summary>
    public T? Stage { get; private set; }

    /// <summary>
    /// Consumes a message, stages it as the most recent message, and broadcasts it to all observers.
    /// If there are any pending tasks waiting for the next event (via <see cref="WaitNextEvent"/>),
    /// they will be completed with the consumed message.
    /// </summary>
    /// <param name="value">The message to be consumed, staged, and broadcasted.</param>
    /// <returns>A task that represents the asynchronous operation of broadcasting the message.</returns>
    public async Task Consume(T value)
    {
        if (!IsStaged)
        {
            IsStaged = true;
        }

        Stage = value;
        await BroadcastAsync(value);

        lock (_lock)
        {
            if (_tcs != null && !_tcs.Task.IsCompleted)
            {
                _tcs.SetResult(value);
                _tcs = null; // Reset for the next event
            }
        }
    }

    /// <summary>
    /// Waits asynchronously for the next message to be consumed.
    /// This method ignores the currently staged message and only completes when a new message is consumed.
    /// </summary>
    /// <returns>
    /// A task that completes when the next message is consumed, returning the new message.
    /// </returns>
    public Task<T> WaitNextEvent()
    {
        lock (_lock)
        {
            if (_tcs == null || _tcs.Task.IsCompleted)
            {
                _tcs = new TaskCompletionSource<T>();
            }

            return _tcs.Task;
        }
    }

    /// <summary>
    /// Retrieves the currently staged message if one exists, or waits asynchronously for the next message to be consumed.
    /// If a message has already been consumed and staged, this method returns it immediately.
    /// Otherwise, it behaves like <see cref="WaitNextEvent"/> and waits for a new message.
    /// </summary>
    /// <returns>
    /// A task that completes with the currently staged message if available, 
    /// or with the next consumed message if no message is staged.
    /// </returns>
    public Task<T> WaitOneEvent()
    {
        if (IsStaged)
        {
            return Task.FromResult(Stage!);
        }

        return WaitNextEvent();
    }
}