namespace Aiursoft.AiurObserver.Features;

public class FilteredObservable<T>(IAsyncObservable<T> source, Func<T, bool> predicate) : IAsyncObservable<T>
{
    public ISubscription Subscribe(IConsumer<T> observer)
    {
        var consumer = new Consumer<T>(async value =>
        {
            if (predicate(value))
            {
                await observer.Consume(value);
            }
        });
        return source.Subscribe(consumer);
    }
}