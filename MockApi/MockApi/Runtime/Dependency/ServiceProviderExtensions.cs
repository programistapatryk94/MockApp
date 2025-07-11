namespace MockApi.Runtime.Dependency
{
    public static class ServiceProviderExtensions
    {
        public static IDisposableDependencyWrapper<T> ResolveAsDisposable<T>(this IServiceProvider serviceProvider, Type implementationType)
        {
            var scope = serviceProvider.CreateScope();
            var service = (T)scope.ServiceProvider.GetRequiredService(implementationType);
            return new DisposableDependencyWrapper<T>(scope, service);
        }

        public static IDisposableDependencyWrapper<T> ResolveAsDisposable<T>(this IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var service = (T)scope.ServiceProvider.GetRequiredService(typeof(T));
            return new DisposableDependencyWrapper<T>(scope, service);
        }
    }
}
