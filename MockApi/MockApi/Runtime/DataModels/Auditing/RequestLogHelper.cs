using MockApi.Data;
using MockApi.Extensions;
using MockApi.Models;
using MockApi.Runtime.Session;
using System.Net;
using System.Reflection;

namespace MockApi.Runtime.DataModels.Auditing
{
    public class RequestLogHelper : IRequestLogHelper
    {
        private readonly IRequestLogConfiguration _requestLogConfiguration;
        private readonly IAppSession _appSession;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditSerializer _auditSerializer;
        private readonly AppDbContext _context;

        public RequestLogHelper(IRequestLogConfiguration requestLogConfiguration, IAppSession appSession, IHttpContextAccessor httpContextAccessor, IAuditSerializer auditSerializer, AppDbContext context)
        {
            _requestLogConfiguration = requestLogConfiguration;
            _appSession = appSession;
            _httpContextAccessor = httpContextAccessor;
            _auditSerializer = auditSerializer;
            _context = context;
        }

        public RequestLog CreateAuditLog(Type type, MethodInfo method, IDictionary<string, object> arguments)
        {
            var requestLog = new RequestLog
            {
                UserId = _appSession.UserId,
                ServiceName = (type != null
                    ? type.FullName
                    : string.Empty).TruncateWithPostfix(RequestLog.MaxServiceNameLength),
                MethodName = method.Name.TruncateWithPostfix(RequestLog.MaxMethodNameLength),
                Parameters = ConvertArgumentsToJson(arguments).TruncateWithPostfix(RequestLog.MaxParametersLength),
                ExecutionTime = DateTime.UtcNow,
                ClientIpAddress = GetClientIpAddress().TruncateWithPostfix(RequestLog.MaxClientIpAddressLength),
                ClientName = GetComputerName().TruncateWithPostfix(RequestLog.MaxClientNameLength), // lub inna identyfikacja
                BrowserInfo = GetBrowserInfo().TruncateWithPostfix(RequestLog.MaxBrowserInfoLength)
            };

            return requestLog;
        }

        public bool ShouldSaveLog(MethodInfo methodInfo, bool defaultValue = false)
        {
            if (!_requestLogConfiguration.IsEnabled)
            {
                return false;
            }

            if (!_requestLogConfiguration.IsEnabledForAnonymousUsers && (_appSession?.UserId == null))
            {
                return false;
            }

            if (null == methodInfo)
            {
                return false;
            }

            if (!methodInfo.IsPublic)
            {
                return false;
            }

            if (methodInfo.IsDefined(typeof(EnableLogAttribute), true))
            {
                return true;
            }

            if (methodInfo.IsDefined(typeof(DisableLogAttribute), true))
            {
                return false;
            }

            var classType = methodInfo.DeclaringType;
            if (classType != null)
            {
                if (classType.GetTypeInfo().IsDefined(typeof(EnableLogAttribute), true))
                {
                    return true;
                }

                if (classType.GetTypeInfo().IsDefined(typeof(DisableLogAttribute), true))
                {
                    return false;
                }
            }

            return defaultValue;
        }

        protected string GetBrowserInfo()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.Request?.Headers?["User-Agent"];
        }

        protected virtual string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string ip = string.Empty;

                // X-Forwarded-For: client, proxy1, proxy2
                if (httpContext?.Request?.Headers != null &&
                    httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
                {
                    var forwardedForValue = forwardedFor.FirstOrDefault();
                    if (!string.IsNullOrEmpty(forwardedForValue))
                    {
                        ip = forwardedForValue.Split(',')[0].Trim();
                    }
                }

                // X-Real-IP: client
                if (string.IsNullOrEmpty(ip) &&
                    httpContext?.Request?.Headers != null &&
                    httpContext.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
                {
                    ip = realIp.FirstOrDefault();
                }

                // RemoteIpAddress
                if (string.IsNullOrEmpty(ip) &&
                    httpContext?.Connection?.RemoteIpAddress != null)
                {
                    ip = httpContext.Connection.RemoteIpAddress.ToString();
                }

                // Check if it's a valid IP
                if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out _))
                {
                    return ip;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private string GetComputerName()
        {
            return null;
        }

        private string ConvertArgumentsToJson(IDictionary<string, object> arguments)
        {
            try
            {
                if (arguments.IsNullOrEmpty())
                {
                    return "{}";
                }

                var dictionary = new Dictionary<string, object>();

                foreach (var argument in arguments)
                {
                    if (argument.Value != null && _requestLogConfiguration.IgnoredTypes.Any(t => t.IsInstanceOfType(argument.Value)))
                    {
                        dictionary[argument.Key] = null;
                    }
                    else
                    {
                        dictionary[argument.Key] = argument.Value;
                    }
                }

                return _auditSerializer.Serialize(dictionary);
            }
            catch (Exception ex)
            {
                return "{}";
            }
        }

        public async Task SaveAsync(RequestLog log)
        {
            _context.RequestLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
