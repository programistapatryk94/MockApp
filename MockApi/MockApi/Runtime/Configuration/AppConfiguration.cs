using MockApi.Runtime.Exceptions.Handling;

namespace MockApi.Runtime.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public WrapResultAttribute DefaultWrapResultAttribute { get; }

        public AppConfiguration()
        {
            DefaultWrapResultAttribute = new WrapResultAttribute();
        }
    }
}
