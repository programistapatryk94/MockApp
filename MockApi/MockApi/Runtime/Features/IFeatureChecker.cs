namespace MockApi.Runtime.Features
{
    public interface IFeatureChecker
    {
        Task<string> GetValueAsync(string name);
        string GetValue(string name);
        Task<bool> IsEnabledAsync(string name);
        Task<string> GetValueAsync(Guid userId, string name);
        Task<bool> IsEnabledAsync(Guid userId, string name);
    }
}
