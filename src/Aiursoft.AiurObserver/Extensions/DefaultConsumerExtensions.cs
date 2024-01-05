using System.Numerics;

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
    
    public static MessageSum<T> Adder<T>(this IAsyncObservable<T> source) where T : struct, INumber<T>
    {
        return new MessageSum<T>(source);
    }
    
    public static RecentMessageAverage<T> AverageRecent<T>(this IAsyncObservable<T> source, int recent) where T : struct, INumber<T>, IDivisionOperators<T, T, T>
    {
        return new RecentMessageAverage<T>(source, recent);
    }
    
    public static MessageAverage<T> Average<T>(this IAsyncObservable<T> source) where T : struct, INumber<T>
    {
        return new MessageAverage<T>(source);
    }
}