namespace Aiursoft.AiurObserver
{
    /// <summary>
    /// Represents a subscription to an event or observable sequence that can be unsubscribed.
    /// </summary>
    public class Subscription : ISubscription, IDisposable
    {
        private readonly Action _unsubscribeAction;

        internal Subscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }

        public void Unsubscribe(bool throwIfAlreadyUnsubscribed = false)
        {
            try
            {
                _unsubscribeAction();
            }
            catch
            {
                if (throwIfAlreadyUnsubscribed)
                {
                    throw;
                }
            }
        }
        
        public void Dispose()
        {
            Unsubscribe();
        }
    }
}
