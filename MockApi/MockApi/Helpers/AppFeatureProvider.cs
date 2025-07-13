using MockApi.Runtime.Features;

namespace MockApi.Helpers
{
    public class AppFeatureProvider : FeatureProvider
    {
        public override void SetFeatures(IFeatureDefinitionContext context)
        {
            context.Create(AppFeatures.MaxProjectCreationLimit, "1");

            context.Create(AppFeatures.MaxMockCreationLimit, "3");

            context.Create(AppFeatures.CollaborationEnabled, "false");

            context.Create(AppFeatures.DefaultLanguage, "en");
        }
    }
}
