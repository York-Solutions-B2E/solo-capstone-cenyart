using Shared.DTOs;
using Shared.Interfaces;

namespace WebApi.Services;

public class EventService(ICommunicationService service) : IEventService
{
    private readonly ICommunicationService _service = service;

    public async Task PublishAsync(EventDto dto)
    {
        await _service.UpdateStatusAsync(dto.CommunicationId, dto.StatusCode);
    }
}
