using System.Numerics;

namespace Aiursoft.AiurObserver;

public class RecentMessageAverage<T> where T : struct, INumber<T>
{
    private int Count { get; set; }
    
    private readonly int _recent;
    private readonly T[] _buffer;
    
    public RecentMessageAverage(IAsyncObservable<T> source, int recent)
    {
        _buffer = new T[recent];
        _recent = recent;
        source.Subscribe(item =>
        {
            _buffer[Count % recent] = item;
            Count++;
            return Task.CompletedTask;
        });
    }
    
    public (T Sum, int count) Average()
    {
        T sum = default;
        for (int i = 0; i < _buffer.Length; i++)
        {
            sum += _buffer[i];
        }

        return (sum, Math.Min(Count, _recent));
    }
}