using WorkTracker.Common.Constants;

namespace WorkTracker.Common.IntegrationEvents
{
    /// <summary>
    /// Published when a user successfully authenticates.
    /// Exchange: worktracker.events  |  Routing key: user.loggedin
    /// </summary>
    public sealed record UserLoggedInEvent : CloudEvent<UserLoggedInData>
    {
        public const string EventType = RabbitMqKeys.UserEventType;
        public const string EventSource = RabbitMqKeys.UserEventSource;
        public const string RoutingKey = RabbitMqKeys.UserRoutingKey;
        public const string QueueName = RabbitMqKeys.UserQueueName;

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
