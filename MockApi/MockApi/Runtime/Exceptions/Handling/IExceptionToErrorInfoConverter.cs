namespace MockApi.Runtime.Exceptions.Handling
{
    public interface IExceptionToErrorInfoConverter
    {
        ErrorInfo Convert(Exception exception);
    }
}
