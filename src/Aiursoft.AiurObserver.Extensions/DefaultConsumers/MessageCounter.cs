﻿namespace Aiursoft.AiurObserver.DefaultConsumers;

public class MessageCounter<T> : AsyncObservable<int>, IConsumer<T>
{
    public int Count { get; private set; }
    
    public async Task Consume(T value)
    {
        lock (this)
        {
            Count++;
        }
        await BroadcastAsync(Count);
    }
}