using Shared.Dtos;

namespace BlazorServer.Services
{
    public class CommService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        /// <summary>
        /// Fetches a page of communications along with the total count.
        /// </summary>
        public async Task<(IEnumerable<Dto> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var url = $"api/comm?page={page}&pageSize={pageSize}";

            // Internal type to bind the API's JSON shape
            var wrapper = await _http.GetFromJsonAsync<PaginatedResultWrapper>(url)
                          ?? new PaginatedResultWrapper();

            return (wrapper.Items, wrapper.TotalCount);
        }

        /// <summary>
        /// Fetches all communications (unpaginated).
        /// </summary>
        public async Task<IEnumerable<Dto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<IEnumerable<Dto>>("api/comm/all");
            return result ?? Array.Empty<Dto>();
        }

        /// <summary>
        /// Fetches the details (including history) of one communication.
        /// </summary>
        public async Task<DetailsDto> GetByIdAsync(Guid id)
        {
            var result = await _http.GetFromJsonAsync<DetailsDto>($"api/comm/{id}");
            if (result == null)
                throw new InvalidOperationException($"Communication with ID {id} not found.");
            return result;
        }

        /// <summary>
        /// Creates a new communication.
        /// </summary>
        public async Task CreateAsync(CommunicationCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/comm", dto);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Updates a communication's status.
        /// </summary>
        public async Task UpdateAsync(CommunicationUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync("api/comm", dto);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Soft‐deletes a communication.
        /// </summary>
        public async Task DeleteAsync(CommunicationDeleteDto dto)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, "api/comm")
            {
                Content = JsonContent.Create(dto)
            };
            var response = await _http.SendAsync(req);
            response.EnsureSuccessStatusCode();
        }

        // Internal class to match the API's paginated response shape.
        private class PaginatedResultWrapper
        {
            public List<Dto> Items { get; set; } = new();
            public int TotalCount { get; set; }
        }
    }
}
