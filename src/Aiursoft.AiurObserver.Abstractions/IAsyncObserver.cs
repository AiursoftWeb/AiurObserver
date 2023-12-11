namespace Aiursoft.AiurObserver
{
    public interface IAsyncObserver<in T>
    {
        public Func<T, Task> OnNext { get; }
    }
}
