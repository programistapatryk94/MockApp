using MockApi.Runtime.Collections;

namespace MockApi.Runtime.Features
{
    public interface IFeatureConfiguration
    {
        ITypeList<FeatureProvider> Providers { get; }
    }
}
