using Shared.Dtos;

namespace BlazorServer.Services;

public class TypeService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task<List<TypeDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<TypeDto>>("api/types")
           ?? [];

    public async Task<TypeDetailsDto> GetByCodeAsync(string typeCode)
        => await _http.GetFromJsonAsync<TypeDetailsDto>($"api/types/{typeCode}")
           ?? throw new InvalidOperationException("Type not found");

    public async Task CreateAsync(TypeCreateDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/types", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(TypeUpdateDto dto)
    {
        var res = await _http.PutAsJsonAsync("api/types", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(TypeDeleteDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, "api/types")
        {
            Content = JsonContent.Create(dto)
        };
        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
    }
}
