namespace Aiursoft.AiurObserver
{
    public static class Extensions
    {
        public static ISubscription Subscribe<T>(this IAsyncObservable<T> source, Func<T, Task> onHappen)
        {
            return source.Subscribe(new Consumer<T>(onHappen));
        }
        
        public static FilteredObservable<T> Filter<T>(this IAsyncObservable<T> source, Func<T, bool> predicate)
        {
            return new FilteredObservable<T>(source, predicate);
        }
        
        public static MappedObservable<T1, T2> Map<T1, T2>(this IAsyncObservable<T1> source, Func<T1, T2> mapper)
        {
            return new MappedObservable<T1, T2>(source, mapper);
        }
        
        public static MessageState<T> Keep<T>(this IAsyncObservable<T> source)
        {
            return new MessageState<T>(source);
        }
    }
    
    public class FilteredObservable <T> : IAsyncObservable<T>
    {
        private readonly IAsyncObservable<T> _source;
        private readonly Func<T, bool> _predicate;

        public FilteredObservable(IAsyncObservable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        public ISubscription Subscribe(IConsumer<T> observer)
        {
            var consumer = new Consumer<T>(async value =>
            {
                if (_predicate(value))
                {
                    await observer.Consume(value);
                }
            });
            return _source.Subscribe(consumer);
        }
    }
    
    public class MappedObservable<T1, T> : IAsyncObservable<T>
    {
        private readonly IAsyncObservable<T1> _source;
        private readonly Func<T1, T> _mapper;

        public MappedObservable(IAsyncObservable<T1> source, Func<T1, T> mapper)
        {
            _source = source;
            _mapper = mapper;
        }

        public ISubscription Subscribe(IConsumer<T> observer)
        {
            return _source.Subscribe(value => observer.Consume(_mapper(value)));
        }
    }
    
    public class MessageState<T>
    {
        public MessageState(IAsyncObservable<T> source)
        {
            source.Subscribe(value =>
            {
                Last = value;
                return Task.CompletedTask;
            });
        }

        public T? Last { get; private set; }
    }
}
