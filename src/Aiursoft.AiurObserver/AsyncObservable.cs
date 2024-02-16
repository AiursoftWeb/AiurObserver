namespace Aiursoft.AiurObserver;

public class AsyncObservable<T> : IAsyncObservable<T>
{
    protected readonly List<IConsumer<T>> Observers = new();
    protected readonly object ObserversEditLock = new();

    public ISubscription Subscribe(IConsumer<T> observer)
    {
        lock (ObserversEditLock)
        {
            if (!Observers.Contains(observer))
            {
                Observers.Add(observer);
            }
            else
            {
                throw new Exception("This observer is already subscribed!");
            }
        }

        return new Subscription(Unsubscribe);

        void Unsubscribe()
        {
            lock (ObserversEditLock)
            {
                if (Observers.Contains(observer))
                {
                    if (!Observers.Remove(observer)) throw new Exception("Failed to remove observer.");
                }
            }
        }
    }

    public void RemoveAllListeners()
    {
        lock (ObserversEditLock)
        {
            Observers.Clear();
        }
    }

    private IEnumerable<Task> PrepareBroadcastTasks(T newEvent)
    {
        lock (ObserversEditLock)
        {
            return Observers.Select(t => t.Consume(newEvent));
        }
    }

    public Task BroadcastAsync(T newEvent)
    {
        return Task.WhenAll(PrepareBroadcastTasks(newEvent));
    }

    public int GetListenerCount()
    {
        lock (ObserversEditLock)
        {
            return Observers.Count;
        }
    }
}