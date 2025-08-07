using Shared.Dtos;

namespace BlazorServer.Services;

public class StatusService(HttpClient http)
{
    private readonly HttpClient _http = http;

    /// <summary>Gets all statuses valid for a given type.</summary>
    public async Task<List<StatusDto>> GetByTypeAsync(string typeCode)
        => await _http.GetFromJsonAsync<List<StatusDto>>($"api/statuses/{typeCode}")
           ?? [];

    public async Task AddAsync(StatusCreateDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/statuses", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(StatusUpdateDto dto)
    {
        var res = await _http.PutAsJsonAsync("api/statuses", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(StatusDeleteDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, "api/statuses")
        {
            Content = JsonContent.Create(dto)
        };
        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }
}
