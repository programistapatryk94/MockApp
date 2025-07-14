using Newtonsoft.Json;

namespace MockApi.Runtime.DataModels.Auditing
{
    public class AuditSerializer : IAuditSerializer
    {
        private readonly IRequestLogConfiguration _requestLogConfiguration;

        public AuditSerializer(IRequestLogConfiguration requestLogConfiguration)
        {
            _requestLogConfiguration = requestLogConfiguration;
        }

        public string Serialize(object obj)
        {
            var options = new JsonSerializerSettings
            {
                ContractResolver = new AuditingContractResolver(_requestLogConfiguration.IgnoredTypes)
            };

            return JsonConvert.SerializeObject(obj, options);
        }
    }
}
