using Shared.Dtos;

namespace BlazorServer.Services;

public class TypeService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<TypeDto>> GetAllTypesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<TypeDto>>("api/type");
        return result ?? new List<TypeDto>();
    }

    public async Task<TypeDetailsDto?> GetTypeByCodeAsync(string typeCode)
    {
        return await _httpClient.GetFromJsonAsync<TypeDetailsDto>($"api/type/{typeCode}");
    }

    public async Task CreateTypeAsync(CreateTypePayload payload)
    {
        var response = await _httpClient.PostAsJsonAsync("api/type", payload);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateTypeAsync(UpdateTypePayload payload)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/type/{payload.TypeCode}", payload);
        response.EnsureSuccessStatusCode();
    }

    public async Task SoftDeleteTypeAsync(DeleteTypePayload payload)
    {
        var response = await _httpClient.DeleteAsync($"api/type/{payload.TypeCode}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> ValidateStatusesForTypeAsync(string typeCode, List<string> statusCodes)
    {
        var payload = new ValidateStatusesPayload(typeCode, statusCodes);
        var response = await _httpClient.PostAsJsonAsync("api/type/validate-statuses", payload);
        return await response.Content.ReadFromJsonAsync<bool>();
    }
}
