namespace MockApi.Services
{
    public interface IUserManager
    {
        Task SetFeatureValueAsync(Guid userId, string featureName, string value);
        Task RemoveFeatureAsync(Guid userId, string featureName);
        Task SetFeatureValuesAsync(Guid userId, Dictionary<string, string> features);
        Task RemoveFeaturesAsync(Guid userId, List<string> features);
        Task<Dictionary<string, string>> GetAllFeaturesAsync(Guid userId);
    }
}
