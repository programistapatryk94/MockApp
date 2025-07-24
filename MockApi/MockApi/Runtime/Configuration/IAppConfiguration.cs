using MockApi.Runtime.Exceptions.Handling;

namespace MockApi.Runtime.Configuration
{
    public interface IAppConfiguration
    {
        WrapResultAttribute DefaultWrapResultAttribute { get; }
    }
}
