namespace MockApi.Runtime.Exceptions.Handling
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class WrapExceptionAttribute : Attribute
    {
        public bool LogError { get; set; }
        public bool WrapOnError { get; set; }

        public WrapExceptionAttribute(bool wrapOnError = true)
        {
            WrapOnError = wrapOnError;

            LogError = true;
        }
    }
}
