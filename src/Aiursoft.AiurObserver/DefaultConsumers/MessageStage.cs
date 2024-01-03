namespace Aiursoft.AiurObserver;

public class MessageStage<T>
{
    public MessageStage(IAsyncObservable<T> source)
    {
        source.Subscribe(value =>
        {
            IsStaged = true;
            Stage = value;
            return Task.CompletedTask;
        });
    }
    
    public bool IsStaged { get; private set; }

    public T? Stage { get; private set; }
}

public class MessageStageFirst<T>
{
    public MessageStageFirst(IAsyncObservable<T> source)
    {
        source.Subscribe(value =>
        {
            if (!IsStaged)
            {
                Stage = value;
                IsStaged = true;
            }
            return Task.CompletedTask;
        });
    }

    public bool IsStaged { get; private set; }

    public T? Stage { get; private set; }
}