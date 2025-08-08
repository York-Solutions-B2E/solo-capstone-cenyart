using Shared.Dtos;

namespace BlazorServer.Services
{
    public class TypeService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        /// <summary>
        /// Fetches all communication types.
        /// </summary>
        public async Task<IEnumerable<TypeDto>> GetAllAsync()
        {
            var result = await _http.GetFromJsonAsync<IEnumerable<TypeDto>>("api/type");
            return result ?? Array.Empty<TypeDto>();
        }

        /// <summary>
        /// Fetch details about a single type, including its valid statuses.
        /// </summary>
        public async Task<TypeDetailsDto> GetByCodeAsync(string typeCode)
        {
            if (string.IsNullOrWhiteSpace(typeCode))
                throw new ArgumentException("Type code is required", nameof(typeCode));

            var result = await _http.GetFromJsonAsync<TypeDetailsDto>($"api/type/{typeCode}");
            if (result == null)
                throw new InvalidOperationException($"Type '{typeCode}' not found.");

            return result;
        }

        /// <summary>
        /// Creates a new communication type.
        /// </summary>
        public async Task CreateAsync(TypeCreateDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/type", dto);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Updates an existing communication type.
        /// </summary>
        public async Task UpdateAsync(TypeUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync("api/type", dto);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Deletes a communication type.
        /// </summary>
        public async Task DeleteAsync(TypeDeleteDto dto)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "api/type")
            {
                Content = JsonContent.Create(dto)
            };
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
