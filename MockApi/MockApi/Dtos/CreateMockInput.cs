namespace MockApi.Dtos
{
    public class CreateMockInput
    {
        public string UrlPath { get; set; } = "";
        public string Method { get; set; } = "GET";
        public int StatusCode { get; set; } = 200;
        public string ResponseBody { get; set; } = "{}";
        public string? HeadersJson { get; set; }
    }
}
