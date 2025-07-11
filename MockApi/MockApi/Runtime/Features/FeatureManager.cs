
using MockApi.Runtime.Dependency;
using System.Collections.Immutable;

namespace MockApi.Runtime.Features
{
    public class FeatureManager : FeatureDefinitionContextBase, IFeatureManager
    {
        private readonly IFeatureConfiguration _featureConfiguration;
        private readonly IServiceProvider _serviceProvider;

        public FeatureManager(IFeatureConfiguration featureConfiguration, IServiceProvider serviceProvider)
        {
            _featureConfiguration = featureConfiguration;
            _serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            foreach (var providerType in _featureConfiguration.Providers)
            {
                using (var provider = CreateProvider(providerType))
                {
                    provider.Object.SetFeatures(this);
                }
            }

            Features.AddAllFeatures();
        }

        public Feature Get(string name)
        {
            var feature = GetOrNull(name);
            if (feature == null)
            {
                throw new Exception("There is no feature with name: " + name);
            }

            return feature;
        }

        public IReadOnlyList<Feature> GetAll()
        {
            return Features.Values.ToImmutableList();
        }

        private IDisposableDependencyWrapper<FeatureProvider> CreateProvider(Type providerType)
        {
            return _serviceProvider.ResolveAsDisposable<FeatureProvider>(providerType);
        }
    }
}
