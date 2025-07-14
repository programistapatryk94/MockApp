using Microsoft.EntityFrameworkCore;
using MockApi.Data;

namespace MockApi.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;

        public SubscriptionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsProjectOwnedBySubscribedUser(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.CreatorUser)
                .ThenInclude(p => p.Subscription)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (null == project) return false;

            var sub = project.CreatorUser.Subscription;
            throw new NotImplementedException("Not implemenete");

            //return sub != null && (sub.EndsAt == null || sub.EndsAt > DateTime.UtcNow);
        }
    }
}
