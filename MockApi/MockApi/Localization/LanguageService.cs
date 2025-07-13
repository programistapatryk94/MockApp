using Microsoft.EntityFrameworkCore;
using MockApi.Data;
using MockApi.Helpers;
using MockApi.Runtime.Features;

namespace MockApi.Localization
{
    public class LanguageService : ILanguageService
    {
        private readonly AppDbContext _context;
        private readonly IFeatureManager _featureManager;

        public LanguageService(AppDbContext context, IFeatureManager featureManager)
        {
            _context = context;
            _featureManager = featureManager;
        }

        public async Task ChangeLanguageAsync(string languageName, Guid userId)
        {
            var feature = _featureManager.Get(AppFeatures.DefaultLanguage);
            var defaultLanguage = feature.DefaultValue;
            var userFeature = await _context.FeatureSettings
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Name == AppFeatures.DefaultLanguage);

            if (languageName.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                if (userFeature != null)
                {
                    _context.FeatureSettings.Remove(userFeature);
                }
            }
            else
            {
                if (userFeature != null)
                {
                    userFeature.Value = languageName;
                }
                else
                {
                    _context.FeatureSettings.Add(new Models.FeatureSetting
                    {
                        UserId = userId,
                        Value = languageName,
                        Name = AppFeatures.DefaultLanguage,
                        CreatorUserId = userId,
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
