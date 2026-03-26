using AutomationService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace AutomationService.Api.Controllers;

[ApiController]
[Route("api/events/{aquariumId:guid}")]
public sealed class EventController(IOutboxMessageRepository outboxMessageRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetEvents(
        [FromRoute] Guid aquariumId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var normalizedTake = Math.Clamp(take, 1, 500);
        var events = await outboxMessageRepository.GetHistoryAsync(aquariumId, normalizedTake, cancellationToken);

        return Ok(events.Select(x => new
        {
            x.Id,
            x.AquariumId,
            x.Type,
            x.Payload,
            x.OccurredAtUtc,
            x.ProcessedAtUtc,
            x.Error
        }));
    }
}
