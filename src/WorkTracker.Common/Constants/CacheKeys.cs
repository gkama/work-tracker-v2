namespace WorkTracker.Common.Constants
{
    public static class CacheKeys
    {
        private const string WorkTrackerPrefix = "worktracker";
        private const string WorkTrackerUsers = "users";

        public static string GetWorkTrackerKey(params string[] segments)
        {
            return string.Join("_", new[] { WorkTrackerPrefix }.Concat(segments));
        }

        public static string GetUserKey(string username) =>
            GetWorkTrackerKey([
                WorkTrackerUsers,
                username]);
    }
}