namespace Aiursoft.AiurObserver
{
    /// <summary>
    /// Represents a subscription to an event or observable sequence that can be unsubscribed.
    /// </summary>
    public class Subscription : ISubscription, IDisposable
    {
        private readonly Action _unsubscribeAction;

        public Subscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }

        /// <summary>
        /// Unsubscribes from an action.
        /// </summary>
        public void Unsubscribe()
        {
            _unsubscribeAction();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            Unsubscribe();
        }
    }
}
