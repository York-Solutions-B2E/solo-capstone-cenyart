using Shared.DTOs;
using Shared.Interfaces;

namespace WebApi.Services;
public class EventService(ICommunicationService comm) : IEventService
{
    private readonly ICommunicationService _comm = comm;

    public async Task PublishAsync(EventDto dto)
    {
        var map = dto.EventType switch
        {
            "IdCardPrinted"   => "Printed",
            "IdCardShipped"   => "Shipped",
            "IdCardDelivered" => "Delivered",
            _ => null
        };
        if (map != null)
            await _comm.UpdateStatusAsync(dto.CommunicationId, map);
    }
}
