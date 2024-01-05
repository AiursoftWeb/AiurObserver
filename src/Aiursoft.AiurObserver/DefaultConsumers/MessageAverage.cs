using System.Numerics;

namespace Aiursoft.AiurObserver;

public class MessageAverage<T> where T : struct, INumber<T>
{
    private int Count { get; set; }
    
    private T _sum;
    
    public MessageAverage(IAsyncObservable<T> source)
    {
        source.Subscribe(item =>
        {
            _sum += item;
            Count++;
            return Task.CompletedTask;
        });
    }
    
    public (T Sum, int count) Average()
    {
        return (_sum, Count);
    }
}