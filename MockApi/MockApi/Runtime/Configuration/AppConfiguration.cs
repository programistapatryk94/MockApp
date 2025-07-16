using MockApi.Runtime.Exceptions.Handling;

namespace MockApi.Runtime.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public WrapExceptionAttribute DefaultWrapExceptionAttribute { get; }

        public AppConfiguration()
        {
            DefaultWrapExceptionAttribute = new WrapExceptionAttribute();
        }
    }
}
