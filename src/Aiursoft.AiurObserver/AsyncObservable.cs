namespace Aiursoft.AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        protected readonly List<IAsyncObserver<T>> Observers = new();
        private readonly object _lock = new();

        public ISubscription Subscribe(IAsyncObserver<T> observer)
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

            return new AsyncSubscription(unRegisterAction: () =>
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
            return Observers.Select(t => t.OnNext(newEvent));
        }
        
        public async Task BroadcastAsync(T newEvent)
        {
            await Task.WhenAll(Broadcast(newEvent));
        }

        public int GetListenerCount()
        {
            return Observers.Count;
        }
    }
}