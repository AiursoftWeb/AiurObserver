using System.Numerics;

namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageAdder<T> : AsyncObservable<T>, IConsumer<T> where T : struct, INumber<T>
{
    public T Sum { get; private set; }

    public async Task Consume(T value)
    {
        Sum += value;
        await BroadcastAsync(Sum);
    }
}