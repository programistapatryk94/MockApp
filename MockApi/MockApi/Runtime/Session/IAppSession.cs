namespace MockApi.Runtime.Session
{
    public interface IAppSession
    {
        Guid? UserId { get; }
    }
}
