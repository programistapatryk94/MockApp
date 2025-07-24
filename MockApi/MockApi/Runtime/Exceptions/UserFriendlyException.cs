using MockApi.Runtime.Exceptions.Handling;

namespace MockApi.Runtime.Exceptions
{
    [Serializable]
    public class UserFriendlyException : AppException, IHasErrorCode
    {
        public UserFriendlyException()
        {

        }

        public UserFriendlyException(string message)
            : base(message)
        {

        }

        public UserFriendlyException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public int Code { get; set; }
    }
}
