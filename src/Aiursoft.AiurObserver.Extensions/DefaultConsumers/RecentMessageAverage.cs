using System.Numerics;

namespace Aiursoft.AiurObserver.DefaultConsumers;

public class RecentMessageAverage<T> 
    : AsyncObservable<(T, int)>, IConsumer<T>
    where T : struct, INumber<T>
{
    private int Count { get; set; }
    
    private readonly int _recent;
    private readonly T[] _buffer;
    
    public RecentMessageAverage(int recent)
    {
        _buffer = new T[recent];
        _recent = recent;
    }
    
    public (T Sum, int count) Average()
    {
        var sum = _buffer.Aggregate<T, T>(default, (current, t) => current + t);
        return (sum, Math.Min(Count, _recent));
    }

    public async Task Consume(T item)
    {
        _buffer[Count % _recent] = item;
        Count++;
        await BroadcastAsync(Average());
    }
}