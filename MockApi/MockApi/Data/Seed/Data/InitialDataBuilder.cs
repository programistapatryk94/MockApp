using Microsoft.EntityFrameworkCore;
using MockApi.Models;

namespace MockApi.Data.Seed.Data
{
    public class InitialDataBuilder
    {
        private readonly AppDbContext _context;

        public InitialDataBuilder(AppDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            new DefaultUserCreator(_context).Create();
            new DefaultSubscriptionPlanBuilder(_context).Create();

            _context.SaveChanges();
        }
    }
}
