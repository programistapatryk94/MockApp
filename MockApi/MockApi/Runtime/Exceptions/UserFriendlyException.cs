namespace MockApi.Runtime.Exceptions
{
    [Serializable]
    public class UserFriendlyException : Exception
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
    }
}
