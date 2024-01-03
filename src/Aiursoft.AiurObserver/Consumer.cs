namespace Aiursoft.AiurObserver
{
    internal class Consumer<T> : IConsumer<T>
    {
        public Func<T, Task> Consume { get; }

        internal Consumer(Func<T, Task> onNext)
        {
            Consume = onNext;
        }
    }
}
