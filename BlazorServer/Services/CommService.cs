using Shared.Dtos;

namespace BlazorServer.Services;

public class CommService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public record PaginatedResult<T>(List<T> Items, int TotalCount);

    /// <summary>Gets a page of communications.</summary>
    public async Task<PaginatedResult<Dto>> GetPaginatedAsync(int page, int pageSize)
    {
        var url = $"api/communications?page={page}&pageSize={pageSize}";
        var response = await _http.GetFromJsonAsync<PaginatedResult<Dto>>(url);
        return response ?? new PaginatedResult<Dto>([], 0);
    }

    /// <summary>Gets all communications (for dropdowns, etc.).</summary>
    public async Task<List<Dto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<Dto>>("api/communications/all")
           ?? [];

    /// <summary>Gets one communication’s details.</summary>
    public async Task<DetailsDto> GetByIdAsync(Guid id)
        => await _http.GetFromJsonAsync<DetailsDto>($"api/communications/{id}")
           ?? throw new InvalidOperationException("Communication not found");

    /// <summary>Creates a new communication.</summary>
    public async Task CreateAsync(CommunicationCreateDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/communications", dto);
        res.EnsureSuccessStatusCode();
    }

    /// <summary>Updates a communication’s status.</summary>
    public async Task UpdateAsync(CommunicationUpdateDto dto)
    {
        var res = await _http.PutAsJsonAsync("api/communications", dto);
        res.EnsureSuccessStatusCode();
    }

    /// <summary>Deletes (soft) a communication.</summary>
    public async Task DeleteAsync(CommunicationDeleteDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, "api/communications")
        {
            Content = JsonContent.Create(dto)
        };
        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }
}
