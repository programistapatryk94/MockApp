namespace MockApi.Runtime.Exceptions.Handling
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class WrapResultAttribute : Attribute
    {
        public bool LogError { get; set; }
        public bool WrapOnSuccess { get; set; }
        public bool WrapOnError { get; set; }

        public WrapResultAttribute(bool wrapOnSuccess = true, bool wrapOnError = true)
        {
            WrapOnError = wrapOnError;
            WrapOnSuccess = wrapOnSuccess;
            LogError = true;
        }
    }
}
