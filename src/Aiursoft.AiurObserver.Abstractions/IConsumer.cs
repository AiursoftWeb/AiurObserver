namespace Aiursoft.AiurObserver
{
    public interface IConsumer<in T>
    {
        public Func<T, Task> Consume { get; }
    }
}
