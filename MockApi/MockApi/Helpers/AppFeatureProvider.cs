using MockApi.Runtime.Features;

namespace MockApi.Helpers
{
    public class AppFeatureProvider : FeatureProvider
    {
        public override void SetFeatures(IFeatureDefinitionContext context)
        {
            context.Create<int>(AppFeatures.MaxProjectCreationLimit, "1");

            context.Create<int>(AppFeatures.MaxMockCreationLimit, "3");

            context.Create<bool>(AppFeatures.CollaborationEnabled, "false");

            context.Create<string>(AppFeatures.DefaultLanguage, "en");
        }
    }
}
