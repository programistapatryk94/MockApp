
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

        public async Task<Dictionary<string, string>> GetAllFeaturesAsync(Guid userId)
        {
            var features = await _context.FeatureSettings
                .Where(f => f.UserId == userId)
                .ToDictionaryAsync(f => f.Name, f => f.Value);

            return features;
        }

        public async Task RemoveFeatureAsync(Guid userId, string featureName)
        {
            var feature = await _context.FeatureSettings.FirstOrDefaultAsync(p => p.UserId == userId && p.Name == featureName);

            if (null == feature) return;

            _context.FeatureSettings.Remove(feature);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFeaturesAsync(Guid userId, List<string> features)
        {
            var existing = await _context.FeatureSettings
            .Where(f => f.UserId == userId && features.Contains(f.Name))
            .ToListAsync();

            foreach (var feature in features)
            {
                var current = existing.FirstOrDefault(f => f.Name == feature);
                if (current != null)
                {
                    _context.FeatureSettings.Remove(current);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetFeatureValueAsync(Guid userId, string featureName, string value)
        {
            var feature = await _context.FeatureSettings.FirstOrDefaultAsync(p => p.UserId == userId && p.Name == featureName);

            if (null == feature)
            {
                feature = new Models.FeatureSetting
                {
                    UserId = userId,
                    Name = featureName,
                    Value = value
                };
                _context.FeatureSettings.Add(feature);
            }
            else
            {
                feature.Value = value;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SetFeatureValuesAsync(Guid userId, Dictionary<string, string> features)
        {
            var existing = await _context.FeatureSettings
            .Where(f => f.UserId == userId && features.Keys.Contains(f.Name))
            .ToListAsync();

            foreach (var feature in features)
            {
                var current = existing.FirstOrDefault(f => f.Name == feature.Key);
                if (current == null)
                {
                    _context.FeatureSettings.Add(new Models.FeatureSetting
                    {
                        UserId = userId,
                        Name = feature.Key,
                        Value = feature.Value
                    });
                }
                else
                {
                    current.Value = feature.Value;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
