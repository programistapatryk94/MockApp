namespace MockApi.Runtime.DataModels.Auditing
{
    public interface IRequestLogConfiguration
    {
        bool IsEnabled { get; set; }
        bool IsEnabledForAnonymousUsers { get; set; }
        bool SaveReturnValues { get; set; }
        List<Type> IgnoredTypes { get; }
    }
}
