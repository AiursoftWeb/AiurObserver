namespace Aiursoft.AiurObserver;

public class CountReachObservable<T> : IAsyncObservable<int>
{
    private readonly IAsyncObservable<T> _source;
    private readonly int _count;
    private int _current;

    public CountReachObservable(IAsyncObservable<T> source, int count)
    {
        if (count < 1)
        {
            throw new ArgumentException("Count must be greater than 0.");
        }
        _source = source;
        _count = count;
    }

    public ISubscription Subscribe(IConsumer<int> observer)
    {
        var consumer = new Consumer<T>(_ =>
        {
            _current++;
            return _current == _count ? observer.Consume(_current) : Task.CompletedTask;
        });
        return _source.Subscribe(consumer);
    }
}