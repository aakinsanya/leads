using API.Models;

namespace API.Clients
{    
    public class LeadsApiClient: ILeadsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string? _endpoint;

        public LeadsApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _endpoint = configuration.GetValue<string>("LeadsApi:Endpoint");
        }

        public async Task<IEnumerable<LeadDto>> GetLeadsAsync(int pageNumber, int pageSize, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }
            if (startDate >= endDate)
            {
                throw new ArgumentException("startDate must be before endDate.");
            }

            // Build the query parameters
            var queryParams = new Dictionary<string, string?>
            {
                { "pageNumber", pageNumber.ToString() },
                { "pageSize", pageSize.ToString() }
            };

            if (startDate.HasValue)
            {
                queryParams.Add("startDate", startDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")); // ISO 8601 format with time
            }

            if (endDate.HasValue)
            {
                queryParams.Add("endDate", endDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }

            var queryString = QueryString.Create(queryParams).ToUriComponent();
            var requestUrl = $"{_endpoint}{queryString}";

            // Make the HTTP GET request
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            // Deserialize the JSON response
            var leads = await response.Content.ReadFromJsonAsync<List<LeadDto>>();
            return leads ?? new List<LeadDto>();
        }
    }
}
