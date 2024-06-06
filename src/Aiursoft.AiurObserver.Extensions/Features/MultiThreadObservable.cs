namespace Aiursoft.AiurObserver.Features;

public class MultiThreadObservable<T>(IAsyncObservable<T> source, Action<Exception>? onError) : IAsyncObservable<T>
{
    private readonly Action<Exception> _onError = onError ?? Console.Error.WriteLine;

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T>(async value =>
        {
            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    await observer.Consume(value);
                }
                catch (Exception ex)
                {
                    _onError(ex);
                }
            }, TaskCreationOptions.LongRunning);
        });
        return source.Subscribe(consumer);
    }
}