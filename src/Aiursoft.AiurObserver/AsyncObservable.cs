namespace Aiursoft.AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        protected readonly List<IConsumer<T>> Observers = new();
        private readonly object _observersEditLock = new();

        /// <summary>
        /// Subscribes an observer to receive notifications.
        /// </summary>
        /// <typeparam name="T">The type of the value being observed.</typeparam>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>An <see cref="ISubscription"/> object representing the subscription.</returns>
        /// <exception cref="Exception">Thrown when the provided observer is already subscribed.</exception>
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