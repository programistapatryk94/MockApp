using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Runtime.Session;

namespace MockApi.Runtime.Features
{
    public class FeatureChecker : IFeatureChecker
    {
        private readonly IAppSession _appSession;
        private readonly IFeatureManager _featureManager;
        private readonly AppDbContext _context;

        public FeatureChecker(IAppSession appSession, IFeatureManager featureManager, AppDbContext context)
        {
            _appSession = appSession;
            _featureManager = featureManager;
            _context = context;
        }

        public string GetValue(string name, bool fallbackToDefault = true)
        {
            var feature = _featureManager.Get(name);

            var dbValue = GetValueFromDb(feature, _appSession.UserId);

            if(dbValue != null)
            {
                return dbValue;
            }

            if(!fallbackToDefault)
            {
                return null;
            }

            return feature.DefaultValue;
        }

        public async Task<string> GetValueAsync(string name, bool fallbackToDefault = true)
        {
            var feature = _featureManager.Get(name);

            var dbValue = await GetValueFromDbAsync(feature, _appSession.UserId);

            if (dbValue != null)
            {
                return dbValue;
            }

            if (!fallbackToDefault)
            {
                return null;
            }

            return feature.DefaultValue;
        }

        public async Task<string> GetValueAsync(Guid userId, string name, bool fallbackToDefault = true)
        {
            var feature = _featureManager.Get(name);

            var dbValue = await GetValueFromDbAsync(feature, userId);

            if (dbValue != null)
            {
                return dbValue;
            }

            if (!fallbackToDefault)
            {
                return null;
            }

            return feature.DefaultValue;
        }

        public async Task<bool> IsEnabledAsync(string name)
        {
            return string.Equals(await GetValueAsync(name), "true", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> IsEnabledAsync(Guid userId, string name)
        {
            return string.Equals(await GetValueAsync(userId, name), "true", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string?> GetValueFromDbAsync(Feature feature, Guid? userId)
        {
            if (!userId.HasValue)
            {
                return null;
            }
            var dbItem = await _context.FeatureSettings
                .Where(p => p.Name == feature.Name && p.UserId == userId)
                .FirstOrDefaultAsync();

            return dbItem?.Value;
        }

        private string? GetValueFromDb(Feature feature, Guid? userId)
        {
            if (!userId.HasValue)
            {
                return null;
            }
            var dbItem = _context.FeatureSettings
                .Where(p => p.Name == feature.Name && p.UserId == userId)
                .FirstOrDefault();

            return dbItem?.Value;
        }
    }
}
