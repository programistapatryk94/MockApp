namespace MockApi.Runtime.Features
{
    public interface IFeatureDefinitionContext
    {
        Feature Create(string name, string defaultValue, Type valueType = null!);
        Feature Create<TType>(string name, string defaultValue);
        Feature GetOrNull(string name);
        void Remove(string name);
    }
}
