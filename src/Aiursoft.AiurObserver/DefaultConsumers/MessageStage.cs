namespace Aiursoft.AiurObserver;

public class MessageStage<T>
{
    public MessageStage(IAsyncObservable<T> source)
    {
        source.Subscribe(value =>
        {
            Stage = value;
            return Task.CompletedTask;
        });
    }

    public T? Stage { get; private set; }
}