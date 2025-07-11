namespace MockApi.Runtime.Features
{
    public abstract class FeatureProvider
    {
        public abstract void SetFeatures(IFeatureDefinitionContext context);
    }
}
