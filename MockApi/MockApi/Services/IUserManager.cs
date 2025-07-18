namespace MockApi.Services
{
    public interface IUserManager
    {
        Task SetFeatureValueAsync(Guid userId, string featureName, string value);
    }
}
