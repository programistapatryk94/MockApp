using Microsoft.EntityFrameworkCore;
using MockApi.Data.Seed.Data;
using System.Transactions;

namespace MockApi.Data.Seed
{
    public static class SeedData
    {
        public static void EnsureSeedData(IServiceProvider services)
        {
            WithDbContext<AppDbContext>(services, context =>
            {
                if(context.Database.IsInMemory())
                {
                    return;
                }

                SeedDb(context);
            });
        }

        public static void SeedDb(AppDbContext context)
        {
            new InitialDataBuilder(context).Create();
        }

        private static void WithDbContext<TDbContext>(IServiceProvider services, Action<TDbContext> contextAction) where TDbContext : DbContext
        {
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
            };

            using(var scope = new TransactionScope(
               TransactionScopeOption.Required,
               options,
               TransactionScopeAsyncFlowOption.Enabled
               ))
            {
                var context = services.GetRequiredService<TDbContext>();

                contextAction(context);

                scope.Complete();
            }
        }
    }
}
