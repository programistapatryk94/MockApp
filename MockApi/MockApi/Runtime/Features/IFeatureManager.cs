namespace MockApi.Runtime.Features
{
    public interface IFeatureManager
    {
        Feature Get(string name);
        Feature GetOrNull(string name);
        IReadOnlyList<Feature> GetAll();
    }
}
