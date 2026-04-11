using Aiursoft.AiurObserver.DefaultConsumers;
using System.Numerics;

namespace Aiursoft.AiurObserver
{
    public static class ConsumerExtensions
    {
        public static MessageCounter<T> Counter<T>(this IAsyncObservable<T> source)
        {
            var counter = new MessageCounter<T>();
            source.Subscribe(counter);
            return counter;
        }

        public static MessageStageFirst<T> StageFirst<T>(this IAsyncObservable<T> source)
        {
            var stage = new MessageStageFirst<T>();
            source.Subscribe(stage);
            return stage;
        }

        public static MessageStageLast<T> StageLast<T>(this IAsyncObservable<T> source)
        {
            var stage = new MessageStageLast<T>();
            source.Subscribe(stage);
            return stage;
        }

        public static MessageStageSpecific<T> StageSpecific<T>(this IAsyncObservable<T> source, int index)
        {
            var stage = new MessageStageSpecific<T>(index);
            source.Subscribe(stage);
            return stage;
        }

        public static MessageAdder<T> Adder<T>(this IAsyncObservable<T> source) where T : struct, INumber<T>
        {
            var adder = new MessageAdder<T>();
            source.Subscribe(adder);
            return adder;
        }

        public static MessageAverage<T> Average<T>(this IAsyncObservable<T> source) where T : struct, INumber<T>
        {
            var average = new MessageAverage<T>();
            source.Subscribe(average);
            return average;
        }

        public static RecentMessageAverage<T> AverageRecent<T>(this IAsyncObservable<T> source, int count) where T : struct, INumber<T>
        {
            var average = new RecentMessageAverage<T>(count);
            source.Subscribe(average);
            return average;
        }
    }
}
