namespace MockApi.Services
{
    public interface ISubscriptionService
    {
        Task<bool> IsProjectOwnedBySubscribedUser(Guid projectId);
    }
}
