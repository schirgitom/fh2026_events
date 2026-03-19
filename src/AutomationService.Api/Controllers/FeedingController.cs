using AutomationService.Api.Contracts;
using AutomationService.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AutomationService.Api.Controllers;

[ApiController]
[Route("api/feeding")]
public sealed class FeedingController(IFeedingService feedingService) : ControllerBase
{
    [HttpPost("{aquariumId:guid}")]
    public async Task<IActionResult> FeedNow(Guid aquariumId, CancellationToken cancellationToken)
    {
        var status = await feedingService.FeedNowAsync(aquariumId, cancellationToken);
        return Ok(status);
    }

    [HttpPost("{aquariumId:guid}/interval")]
    public async Task<IActionResult> SetInterval(
        Guid aquariumId,
        [FromBody] SetFeedingIntervalRequest request,
        CancellationToken cancellationToken)
    {
        var status = await feedingService.SetIntervalAsync(aquariumId, request.IntervalMinutes, cancellationToken);
        return Ok(status);
    }
}
