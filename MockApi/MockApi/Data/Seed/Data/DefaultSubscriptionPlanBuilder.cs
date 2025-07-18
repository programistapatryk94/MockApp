using Microsoft.EntityFrameworkCore;
using MockApi.Models;

namespace MockApi.Data.Seed.Data
{
    public class DefaultSubscriptionPlanBuilder
    {
        private readonly AppDbContext _context;

        public DefaultSubscriptionPlanBuilder(AppDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            var defaultPlan = _context.SubscriptionPlans.IgnoreQueryFilters().FirstOrDefault(p => p.Name == SubscriptionPlan.DefaultNames.DefaultPlan);

            if (null == defaultPlan)
            {
                defaultPlan = new SubscriptionPlan
                {
                    Name = SubscriptionPlan.DefaultNames.DefaultPlan,
                    HasCollaboration = true,
                    HasCustomResponse = true,
                    MaxProjects = 10,
                    MaxResources = 60,
                    Prices = new List<SubscriptionPlanPrice>
                    {
                        new SubscriptionPlanPrice
                        {
                            Name = "Domyślny",
                            Currency = "PLN",
                            Amount = 16m,
                            StripePriceId = "price_1RlwmPBApjyVYR6tRiLgSTAj"
                        },
                        new SubscriptionPlanPrice
                        {
                            Name = "Default",
                            Currency = "USD",
                            Amount = 5m,
                            StripePriceId = "price_1RjL60BApjyVYR6tLn120PQe"
                        }
                    }
                };

                _context.SubscriptionPlans.Add(defaultPlan);
                _context.SaveChanges();
            }
        }
    }
}
