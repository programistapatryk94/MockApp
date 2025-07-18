
using Microsoft.EntityFrameworkCore;
using MockApi.Data;

namespace MockApi.Services
{
    public class AppUserManager : IUserManager
    {
        private readonly AppDbContext _context;

        public AppUserManager(AppDbContext context)
        {
            _context = context;
        }

        public async Task SetFeatureValueAsync(Guid userId, string featureName, string value)
        {
            var feature = await _context.FeatureSettings.FirstOrDefaultAsync(p => p.UserId == userId && p.Name == featureName);

            if (null == feature)
            {
                _context.FeatureSettings.Add(new Models.FeatureSetting
                {
                    Name = featureName,
                    Value = value,
                    UserId = userId
                });
            }
            else
            {
                feature.Value = value;
            }

            await _context.SaveChangesAsync();
        }
    }
}
