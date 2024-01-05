namespace Aiursoft.AiurObserver
{
    public class Consumer<T> : IConsumer<T>
    {
        private readonly Func<T, Task> _innerConsume;

        public Consumer(Func<T, Task> onNext)
        {
            _innerConsume = onNext;
        }

        public Task Consume(T value)
        {
            return _innerConsume(value);
        }
    }
}
