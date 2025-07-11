using MockApi.Extensions;
using System.Collections.Immutable;

namespace MockApi.Runtime.Features
{
    public class Feature
    {
        public object this[string key]
        {
            get => Attributes.GetOrDefault(key);
            set => Attributes[key] = value;
        }

        public IDictionary<string, object> Attributes { get; private set; }

        public Feature Parent { get; private set; }

        public string Name { get; private set; }

        public string DefaultValue { get; set; }

        public IReadOnlyList<Feature> Children => _children.ToImmutableList();

        private readonly List<Feature> _children;

        public Feature(string name, string defaultValue)
        {
            Name = name ?? throw new ArgumentNullException("name");
            DefaultValue = defaultValue;

            _children = new List<Feature>();
            Attributes = new Dictionary<string, object>();
        }

        public Feature CreateChildFeature(string name, string defaultValue)
        {
            var feature = new Feature(name, defaultValue) { Parent = this };
            _children.Add(feature);
            return feature;
        }

        public void RemoveChildFeature(string name)
        {
            var featureToRemove = _children.FirstOrDefault(f => f.Name == name);

            _children.Remove(featureToRemove);
        }

        public override string ToString()
        {
            return string.Format("[Feature: {0}]", Name);
        }
    }
}
