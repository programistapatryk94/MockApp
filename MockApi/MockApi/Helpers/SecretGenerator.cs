namespace MockApi.Helpers
{
    public static class SecretGenerator
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
