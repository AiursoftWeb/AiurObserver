namespace Aiursoft.AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        protected readonly List<IConsumer<T>> Observers = new();
        private readonly object _lock = new();

        public ISubscription Subscribe(IConsumer<T> observer)
        {
            lock (_lock)
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
                lock (_lock)
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

        public IEnumerable<Task> Broadcast(T newEvent)
        {
            return Observers.Select(t => t.Consume(newEvent));
        }
        
        public Task BroadcastAsync(T newEvent)
        {
            return Task.WhenAll(Broadcast(newEvent));
        }

        public int GetListenerCount()
        {
            return Observers.Count;
        }
    }
}