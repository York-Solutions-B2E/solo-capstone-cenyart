using Shared.DTOs;

namespace Shared.Interfaces;

public interface IEventService
{
    Task PublishAsync(EventDto dto);
}
