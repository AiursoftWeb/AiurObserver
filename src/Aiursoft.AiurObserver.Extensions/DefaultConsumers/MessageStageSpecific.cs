namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageStageSpecific<T>: AsyncObservable<T>, IConsumer<T>
{
    private readonly int _index;
    private int _current;
    public bool IsStaged { get; private set; }

    public T? Stage { get; private set; }
    
    public MessageStageSpecific(int index)
    {
        _index = index;
    }

    public async Task Consume(T value)
    {
        if (_current == _index)
        {
            Stage = value;
            IsStaged = true;
            await BroadcastAsync(value);
        }
        _current++;
    }
}