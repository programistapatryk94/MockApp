using MockApi.Runtime.Collections;

namespace MockApi.Runtime.Features
{
    public class FeatureConfiguration : IFeatureConfiguration
    {
        public ITypeList<FeatureProvider> Providers { get; }

        public FeatureConfiguration()
        {
            Providers = new TypeList<FeatureProvider>();
        }
    }
}
