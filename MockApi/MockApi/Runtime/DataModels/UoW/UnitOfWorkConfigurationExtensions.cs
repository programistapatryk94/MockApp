using System.Reflection;

namespace MockApi.Runtime.DataModels.UoW
{
    public static class UnitOfWorkConfigurationExtensions
    {
        public static UnitOfWorkAttribute GetUnitOfWorkAttributeOrNull(this IUnitOfWorkConfiguration unitOfWorkConfiguration, MethodInfo methodInfo)
        {
            var attrs = methodInfo.GetCustomAttributes(true).OfType<UnitOfWorkAttribute>().ToArray();
            if (attrs.Length > 0)
            {
                return attrs[0];
            }

            attrs = methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true).OfType<UnitOfWorkAttribute>().ToArray();
            if (attrs.Length > 0)
            {
                return attrs[0];
            }

            return null;
        }
    }
}
