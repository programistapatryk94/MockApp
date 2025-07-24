namespace MockApi.Runtime.Exceptions.Handling
{
    [Serializable]
    public class ErrorInfo
    {
        public int Code { get; set; }
        public string Error { get; set; }

        public ErrorInfo()
        {

        }

        public ErrorInfo(string error)
        {
            Error = error;
        }

        public ErrorInfo(int code, string error)
            : this(error)
        {
            Code = code;
        }

        public bool __appError { get; } = true;
    }
}
