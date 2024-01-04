namespace Aiursoft.AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        protected readonly List<IConsumer<T>> Observers = new();
        private readonly object _observersEditLock = new();

        public ISubscription Subscribe(IConsumer<T> observer)
        {
            lock (_observersEditLock)
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

            return new Subscription(unsubscribeAction: () =>
            {
                lock (_observersEditLock)
                {
                    if (Observers.Contains(observer))
                    {
                        var removed = Observers.Remove(observer);
                        if (!removed) throw new Exception("Failed to remove observer.");
                    }
                    else
                    {
                        throw new Exception("This observer is not subscribed!");
                    }
                }
            });
        }
        
        public void RemoveAllListeners()
        {
            lock (_observersEditLock)
            {
                Observers.Clear();
            }
        }

        private IEnumerable<Task> Broadcast(T newEvent)
        {
            lock (_observersEditLock)
            {
                return Observers.Select(t => t.Consume(newEvent));
            }
        }
        
        public Task BroadcastAsync(T newEvent)
        {
            return Task.WhenAll(Broadcast(newEvent));
        }

        public int GetListenerCount()
        {
            lock (_observersEditLock)
            {
                return Observers.Count;
            }
        }
    }
}