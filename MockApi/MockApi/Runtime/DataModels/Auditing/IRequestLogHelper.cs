using MockApi.Models;
using System.Reflection;

namespace MockApi.Runtime.DataModels.Auditing
{
    public interface IRequestLogHelper
    {
        bool ShouldSaveLog(MethodInfo methodInfo, bool defaultValue = false);
        RequestLog CreateAuditLog(Type type, MethodInfo method, IDictionary<string, object> arguments);
        Task SaveAsync(RequestLog log);
    }
}
