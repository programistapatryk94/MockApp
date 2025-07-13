namespace MockApi.Runtime.Features
{
    public interface IFeatureChecker
    {
        Task<string> GetValueAsync(string name, bool fallbackToDefault = true);
        string GetValue(string name, bool fallbackToDefault = true);
        Task<bool> IsEnabledAsync(string name);
        Task<string> GetValueAsync(Guid userId, string name, bool fallbackToDefault = true);
        Task<bool> IsEnabledAsync(Guid userId, string name);
    }
}
