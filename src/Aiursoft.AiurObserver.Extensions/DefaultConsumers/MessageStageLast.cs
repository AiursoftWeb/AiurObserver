namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageStageLast<T> : AsyncObservable<T>, IConsumer<T>
{
    public bool IsStaged { get; private set; }

    public T? Stage { get; private set; }

    private TaskCompletionSource<T>? _tcs;
    private readonly object _lock = new();

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

    public Task<T> WaitOneEvent()
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
}
