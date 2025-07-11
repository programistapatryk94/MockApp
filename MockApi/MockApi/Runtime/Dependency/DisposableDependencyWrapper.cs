namespace MockApi.Runtime.Dependency
{
    public interface IDisposableDependencyWrapper<T> : IDisposable
    {
        T Object { get; }
    }

    public class DisposableDependencyWrapper<T> : IDisposableDependencyWrapper<T>
    {
        private readonly IServiceScope _scope;
        public T Object { get; }

        public DisposableDependencyWrapper(IServiceScope scope, T obj)
        {
            _scope = scope;
            Object = obj;
        }

        public void Dispose()
        {
            _scope.Dispose(); // zwalnia wszystkie zależności w scope, w tym `Object`
        }
    }

}
