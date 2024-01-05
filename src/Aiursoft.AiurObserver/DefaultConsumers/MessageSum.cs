using System.Numerics;

namespace Aiursoft.AiurObserver;

public class MessageSum<T> where T : struct, INumber<T>
{
    public T Sum { get; private set; }
    public MessageSum(IAsyncObservable<T> source)
    {
        source.Subscribe(item =>
        {
            Sum += item;
            return Task.CompletedTask;
        });
    }
}