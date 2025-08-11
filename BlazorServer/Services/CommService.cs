using Shared.Dtos;

namespace BlazorServer.Services;

public class CommService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<PaginatedResult<CommDto>> GetCommunicationsAsync(int pageNumber, int pageSize)
    {
        var url = $"api/comms?pageNumber={pageNumber}&pageSize={pageSize}";
        var result = await _httpClient.GetFromJsonAsync<PaginatedResult<CommDto>>(url);
        return result ?? new PaginatedResult<CommDto>(new List<CommDto>(), 0, pageNumber, pageSize);
    }

    public async Task<CommDetailsDto?> GetCommunicationByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<CommDetailsDto>($"api/comms/{id}");
    }

    public async Task CreateCommunicationAsync(CreateCommPayload payload)
    {
        var response = await _httpClient.PostAsJsonAsync("api/comms", payload);
        response.EnsureSuccessStatusCode();
    }
}
