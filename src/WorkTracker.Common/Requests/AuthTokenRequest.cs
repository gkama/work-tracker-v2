namespace WorkTracker.Common.Requests
{
    public class AuthTokenRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
