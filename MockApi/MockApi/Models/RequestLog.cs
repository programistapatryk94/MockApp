using MockApi.Runtime.DataModels;
using System.ComponentModel.DataAnnotations;

namespace MockApi.Models
{
    public class RequestLog : Entity<long>
    {
        public const int MaxServiceNameLength = 256;
        public const int MaxMethodNameLength = 256;
        public const int MaxParametersLength = 4096;
        public const int MaxReturnValueLength = 1024;
        public const int MaxClientIpAddressLength = 64;
        public const int MaxClientNameLength = 128;
        public const int MaxBrowserInfoLength = 512;
        public const int MaxExceptionMessageLength = 1024;
        public const int MaxExceptionLength = 2000;

        public Guid? UserId { get; set; }
        [MaxLength(MaxServiceNameLength)]
        public string? ServiceName { get; set; }
        [MaxLength(MaxMethodNameLength)]
        public string? MethodName { get; set; }
        [MaxLength(MaxParametersLength)]
        public string? Parameters { get; set; }
        [MaxLength(MaxReturnValueLength)]
        public string? ReturnValue { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int ExecutionDuration { get; set; }
        [MaxLength(MaxClientIpAddressLength)]
        public string? ClientIpAddress { get; set; }
        [MaxLength(MaxClientNameLength)]
        public string? ClientName { get; set; }
        [MaxLength(MaxBrowserInfoLength)]
        public string? BrowserInfo { get; set; }
        [MaxLength(MaxExceptionMessageLength)]
        public string? ExceptionMessage { get; set; }
        [MaxLength(MaxExceptionLength)]
        public string? Exception { get; set; }

        public static string GetAppException(Exception exception)
        {
            var clearMessage = "";
            switch (exception)
            {
                case null:
                    return null;

                //case AbpValidationException abpValidationException:
                //    clearMessage = "There are " + abpValidationException.ValidationErrors.Count + " validation errors:";
                //    foreach (var validationResult in abpValidationException.ValidationErrors)
                //    {
                //        var memberNames = "";
                //        if (validationResult.MemberNames != null && validationResult.MemberNames.Any())
                //        {
                //            memberNames = " (" + string.Join(", ", validationResult.MemberNames) + ")";
                //        }

                //        clearMessage += "\r\n" + validationResult.ErrorMessage + memberNames;
                //    }

                //    break;

                //case UserFriendlyException userFriendlyException:
                //    clearMessage =
                //        $"UserFriendlyException.Code:{userFriendlyException.Code}\r\nUserFriendlyException.Details:{userFriendlyException.Details}";
                //    break;
            }

            return exception + (string.IsNullOrWhiteSpace(clearMessage) ? "" : "\r\n\r\n" + clearMessage);
        }
    }
}
