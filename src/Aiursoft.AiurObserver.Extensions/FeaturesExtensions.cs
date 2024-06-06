using Aiursoft.AiurObserver.Features;

namespace Aiursoft.AiurObserver
{
    public static class FeaturesExtensions
    {
        public static FilteredObservable<T> Filter<T>(this IAsyncObservable<T> source, Func<T, bool> predicate)
        {
            return new FilteredObservable<T>(source, predicate);
        }

        public static MappedObservable<T1, T2> Map<T1, T2>(this IAsyncObservable<T1> source, Func<T1, T2> mapper)
        {
            return new MappedObservable<T1, T2>(source, mapper);
        }
        
        public static AsyncMappedObservable<T1, T2> MapAsync<T1, T2>(this IAsyncObservable<T1> source, Func<T1, Task<T2>> mapper)
        {
            return new AsyncMappedObservable<T1, T2>(source, mapper);
        }

        public static MappedObservable<T, T> Pipe<T>(this IAsyncObservable<T> source, Action<T> action)
        {
            return source.Map(t => 
            {
                action(t);
                return t;
            });
        }

        public static ThrottledObservable<T> Throttle<T>(this IAsyncObservable<T> source, TimeSpan throttleTime)
        {
            return new ThrottledObservable<T>(source, throttleTime);
        }
        
        public static RepeatableObservable<T> Repeat<T>(this IAsyncObservable<T> source, int times)
        {
            return new RepeatableObservable<T>(source, times);
        }
        
        public static SampleObservable<T> Sample<T>(this IAsyncObservable<T> source, int every)
        {
            return new SampleObservable<T>(source, every);
        }
        
        public static SampleDoObservable<T> SampleDo<T>(this IAsyncObservable<T> source, int every, Func<T, Task> action)
        {
            return new SampleDoObservable<T>(source, every, action);
        }
        
        public static AggregateObservable<T> Aggregate<T>(this IAsyncObservable<T> source, int every)
        {
            return new AggregateObservable<T>(source, every);
        }
        
        public static SplitObservable<T> ForEach<T>(this IAsyncObservable<T[]> source)
        {
            return new SplitObservable<T>(source);
        }
        
        public static MultiThreadObservable<T> InNewThread<T>(this IAsyncObservable<T> source, Action<Exception>? onError = null)
        {
            return new MultiThreadObservable<T>(source, onError);
        }
        
        public static LockedObservable<T> LockOneThread<T>(this IAsyncObservable<T> source)
        {
            return new LockedObservable<T>(source);
        }
        
        public static DelayedObservable<T> Delay<T>(this MultiThreadObservable<T> source, TimeSpan delay)
        {
            return new DelayedObservable<T>(source, delay);
        }
        
        public static BufferObservable<T> WithBuffer<T>(this IAsyncObservable<T> source, int maxBufferLength)
        {
            return new BufferObservable<T>(source, maxBufferLength);
        }
    }
}