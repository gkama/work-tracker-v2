using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkTracker.Common.Helpers
{
    public static class JsonHelper
    {
        private static JsonSerializerOptions Options => CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }

        public static string SerializeObject<T>(T obj)
            => JsonSerializer.Serialize(obj, Options);

        public static T? DeserializeObject<T>(string json)
            => JsonSerializer.Deserialize<T>(json, Options);
    }
}
