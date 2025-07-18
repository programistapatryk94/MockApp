using MockApi.Extensions;

namespace MockApi.Runtime.Features
{
    public abstract class FeatureDefinitionContextBase : IFeatureDefinitionContext
    {
        protected readonly FeatureDictionary Features;

        protected FeatureDefinitionContextBase()
        {
            Features = new FeatureDictionary();
        }

        public Feature Create(string name, string defaultValue, Type valueType = null!)
        {
            if (Features.ContainsKey(name))
            {
                throw new Exception("There is already a feature with name: " + name);
            }

            var feature = new Feature(name, defaultValue, valueType);
            Features[feature.Name] = feature;
            return feature;
        }

        public Feature Create<TType>(string name, string defaultValue)
        {
            return Create(name, defaultValue, typeof(TType));
        }

        public Feature GetOrNull(string name)
        {
            return Features.GetOrDefault(name);
        }

        public void Remove(string name)
        {
            Features.Remove(name);
        }
    }
}
