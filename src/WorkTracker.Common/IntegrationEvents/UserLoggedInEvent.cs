namespace WorkTracker.Common.IntegrationEvents
{
    /// <summary>
    /// Published when a user successfully authenticates.
    /// Exchange: worktracker.events  |  Routing key: user.loggedin
    /// </summary>
    public sealed record UserLoggedInEvent : CloudEvent<UserLoggedInData>
    {
        public const string EventType = "com.worktracker.user.loggedin";
        public const string EventSource = "/worktracker/auth";
        public const string RoutingKey = "user.loggedin";

        public UserLoggedInEvent(int userId, string username) : base()
        {
            Type = EventType;
            Source = EventSource;
            Subject = userId.ToString();
            Data = new UserLoggedInData(userId, username, DateTimeOffset.UtcNow);
        }
    }

    public sealed record UserLoggedInData(
        int UserId,
        string Username,
        DateTimeOffset LoggedInAt);
}
