namespace Aiursoft.AiurObserver
{
    public interface IAsyncObservable<out T>
    {
        ISubscription Subscribe(IAsyncObserver<T> observer);
    }
}
