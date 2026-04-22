using System.Text.Json.Serialization;

namespace WorkTracker.Common.IntegrationEvents
{
    /// <summary>
    /// Base record conforming to the CloudEvents 1.0 specification.
    /// https://github.com/cloudevents/spec/blob/main/cloudevents/spec.md
    /// </summary>
    public abstract record CloudEvent<TData>
    {
        /// <summary>The version of the CloudEvents specification.</summary>
        [JsonPropertyName("specversion")]
        public string SpecVersion { get; init; } = "1.0";

        /// <summary>Unique identifier for the event.</summary>
        [JsonPropertyName("id")]
        public string Id { get; init; } = Guid.NewGuid().ToString();

        /// <summary>Identifies the context in which the event occurred (URI-reference).</summary>
        [JsonPropertyName("source")]
        public required string Source { get; init; }

        /// <summary>
        /// Reverse-DNS event type descriptor, e.g. com.worktracker.user.loggedin
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        /// <summary>Content type of the <see cref="Data"/> value (always application/json here).</summary>
        [JsonPropertyName("datacontenttype")]
        public string DataContentType { get; init; } = "application/json";

        /// <summary>Identifies the subject of the event within the context of the event producer.</summary>
        [JsonPropertyName("subject")]
        public string? Subject { get; init; }

        /// <summary>Timestamp of when the event occurred (RFC 3339).</summary>
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>Domain-specific event payload.</summary>
        [JsonPropertyName("data")]
        public required TData Data { get; init; }
    }
}
