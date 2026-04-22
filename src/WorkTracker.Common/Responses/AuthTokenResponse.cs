namespace WorkTracker.Common.Responses
{
    public class AuthTokenResponse
    {
        public required string AccessToken { get; set; }
        public required string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }
}
