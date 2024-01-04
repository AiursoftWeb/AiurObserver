namespace Aiursoft.AiurObserver
{
    public class Subscription : ISubscription
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
    }
}
