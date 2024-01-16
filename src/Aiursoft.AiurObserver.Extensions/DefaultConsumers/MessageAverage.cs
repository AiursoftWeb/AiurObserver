using System.Numerics;

namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageAverage<T> : AsyncObservable<(T, int)>, IConsumer<T>
    where T : struct, INumber<T>
{
    private int Count { get; set; }

    private T _sum;

    public (T Sum, int count) Average()
    {
        return (_sum, Count);
    }

    public async Task Consume(T item)
    {
        _sum += item;
        Count++;
        await BroadcastAsync((_sum, Count));
    }
}