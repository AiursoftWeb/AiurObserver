namespace Aiursoft.AiurObserver;

public static class DefaultConsumerExtensions
{
    public static MessageStage<T> Stage<T>(this IAsyncObservable<T> source)
    {
        return new MessageStage<T>(source);
    }

    public static MessageCounter<T> Counter<T>(this IAsyncObservable<T> source)
    {
        return new MessageCounter<T>(source);
    }
}