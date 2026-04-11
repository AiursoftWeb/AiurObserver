namespace Aiursoft.AiurObserver;

public interface ISubscription : IDisposable
{
    public void Unsubscribe();
}