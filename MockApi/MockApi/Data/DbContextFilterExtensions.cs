using Microsoft.EntityFrameworkCore;

namespace MockApi.Data
{
    public static class DbContextFilterExtensions
    {
        public static DbContextFilterScope<TContext> WithFilterOff<TContext>(
            this TContext context,
            string filterPropertyName
        ) where TContext : DbContext
        {
            return new DbContextFilterScope<TContext>(context, filterPropertyName, false);
        }

        public static DbContextFilterScope<TContext> WithFilterOn<TContext>(
            this TContext context,
            string filterPropertyName
        ) where TContext : DbContext
        {
            return new DbContextFilterScope<TContext>(context, filterPropertyName, true);
        }

        public static IDisposable MaybeWithFilterOff<TContext>(
        this TContext context,
        string filterPropertyName,
        bool condition
    ) where TContext : DbContext
        {
            return condition
                ? context.WithFilterOff(filterPropertyName)
                : NullScope.Instance;
        }
    }
}
