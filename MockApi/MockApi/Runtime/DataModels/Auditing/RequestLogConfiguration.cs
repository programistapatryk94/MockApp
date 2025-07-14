
namespace MockApi.Runtime.DataModels.Auditing
{
    public class RequestLogConfiguration : IRequestLogConfiguration
    {
        public bool IsEnabled { get; set; }
        public bool IsEnabledForAnonymousUsers { get; set; }
        public bool SaveReturnValues { get; set; }
        public List<Type> IgnoredTypes { get; }

        public RequestLogConfiguration()
        {
            IsEnabled = true;
            SaveReturnValues = false;
            IgnoredTypes = new List<Type>();
            IsEnabledForAnonymousUsers = false;
        }
    }
}
