namespace Aiursoft.AiurObserver
{
    internal class AsyncObserver<T> : IAsyncObserver<T>
    {
        public Func<T, Task> OnNext { get; }

        internal AsyncObserver(Func<T, Task> onNext)
        {
            OnNext = onNext;
        }
    }
}
