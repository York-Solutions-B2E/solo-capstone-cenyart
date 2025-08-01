using System.Net.Http.Json;
using Shared.DTOs;
using Shared.Interfaces;

namespace BlazorServer.Services;
public class ApiService(HttpClient http) : ICommunicationService, IEventService
{
    private readonly HttpClient _http = http;

    public async Task<List<CommunicationDto>> GetAllAsync()
    {
        var result = await _http.GetFromJsonAsync<List<CommunicationDto>>("api/communications");

        return result ?? []; // or throw if null is unacceptable
    }


    public async Task<CommunicationDto> GetByIdAsync(Guid id)
    {
        // Fetch data
        var result = await _http.GetFromJsonAsync<CommunicationDto>($"api/communications/{id}");

        // Check for null (in case deserialization fails)
        if (result == null)
            throw new InvalidOperationException($"Communication with ID {id} could not be retrieved.");

        return result;
    }


    public async Task<CommunicationDto> CreateAsync(CreateCommunicationDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/communications", dto);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Failed to create communication.");

        var result = await response.Content.ReadFromJsonAsync<CommunicationDto>();

        if (result == null)
            throw new InvalidOperationException("API returned null for a non-nullable contract");

        return result;
    }


    public Task UpdateStatusAsync(Guid id, string s)
        => _http.PutAsJsonAsync($"communications/{id}/status", s);

    public Task SoftDeleteAsync(Guid id)
        => _http.DeleteAsync($"communications/{id}");

    public Task PublishAsync(EventDto dto)
        => _http.PostAsJsonAsync("events", dto);
}
