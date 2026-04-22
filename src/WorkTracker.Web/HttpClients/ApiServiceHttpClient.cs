namespace WorkTracker.Web.HttpClients
{
    public class ApiServiceHttpClient
    {
        private readonly HttpClient _httpClient;

        public ApiServiceHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}

