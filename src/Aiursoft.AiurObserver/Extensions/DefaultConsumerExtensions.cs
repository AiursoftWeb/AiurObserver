namespace Aiursoft.AiurObserver;

public static class DefaultConsumerExtensions
{
    public static MessageStage<T> StageLast<T>(this IAsyncObservable<T> source)
    {
        return new MessageStage<T>(source);
    }

    public static MessageCounter<T> Counter<T>(this IAsyncObservable<T> source)
    {
        return new MessageCounter<T>(source);
    }
    
    public static MessageStageFirst<T> StageFirst<T>(this IAsyncObservable<T> source)
    {
        return new MessageStageFirst<T>(source);
    }
}