namespace Aiursoft.AiurObserver;

public static class BasicExtensions
{
    public static ISubscription Subscribe<T>(this IAsyncObservable<T> source, Func<T, Task> onHappen)
    {
        return source.Subscribe(new Consumer<T>(onHappen));
    }
        
    public static ISubscription Subscribe<T>(this IAsyncObservable<T> source)
    {
        return source.Subscribe(_ => Task.CompletedTask);
    }
}