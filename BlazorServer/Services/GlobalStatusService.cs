using Shared.Dtos;

namespace BlazorServer.Services;

public class GlobalStatusService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<GlobalStatusDto>> GetAllGlobalStatusesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<GlobalStatusDto>>("api/globalstatus");
        return result ?? new List<GlobalStatusDto>();
    }
}
