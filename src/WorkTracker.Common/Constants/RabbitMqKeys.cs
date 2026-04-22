namespace WorkTracker.Common.Constants
{
    public static class RabbitMqKeys
    {
        private const string DefaultEventSource = "/worktracker";

        // Common
        public const string ExchangeName = "worktracker.events";
        public const string ExchangeType = "topic";

        // User
        public const string UserRoutingKey = "user";
        public const string UserEventType = "com.worktracker.user";
        public const string UserEventSource = DefaultEventSource;
        public const string UserQueueName = "worktracker.user";
    }
}
    