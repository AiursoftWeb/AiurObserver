namespace Aiursoft.AiurObserver;

public interface ISubscription
{
    public void Unsubscribe(bool throwIfAlreadyUnsubscribed = false);
}