namespace MockApi.Runtime.Exceptions.Handling
{
    public interface IErrorInfoBuilder
    {
        ErrorInfo BuildForException(Exception exception);
    }
}
