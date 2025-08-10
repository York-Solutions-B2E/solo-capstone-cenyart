using Shared.Dtos;

namespace BlazorServer.Services;

public class TypeService(HttpClient http)
{
    private readonly HttpClient _http = http;

    public async Task<List<TypeDto>> GetAllTypesAsync()
        => await _http.GetFromJsonAsync<List<TypeDto>>("api/types") ?? new List<TypeDto>();

    public async Task<TypeDetailsDto?> GetTypeByCodeAsync(string typeCode)
        => await _http.GetFromJsonAsync<TypeDetailsDto?>($"api/types/{typeCode}");

    public async Task CreateTypeAsync(CreateTypePayload payload)
    {
        var resp = await _http.PostAsJsonAsync("api/types", payload);
        resp.EnsureSuccessStatusCode();
    }

    public async Task UpdateTypeAsync(UpdateTypePayload payload)
    {
        var resp = await _http.PutAsJsonAsync($"api/types/{payload.TypeCode}", payload);
        resp.EnsureSuccessStatusCode();
    }

    public async Task SoftDeleteTypeAsync(DeleteTypePayload payload)
    {
        var resp = await _http.DeleteAsync($"api/types/{payload.TypeCode}");
        resp.EnsureSuccessStatusCode();
    }
}
