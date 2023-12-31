﻿namespace Aiursoft.AiurObserver
{
    public interface IAsyncObservable<out T>
    {
        ISubscription Subscribe(IConsumer<T> observer);
    }
}
