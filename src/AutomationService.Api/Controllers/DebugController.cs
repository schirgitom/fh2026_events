using AutomationService.Application.Abstractions;
using AutomationService.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutomationService.Api.Controllers;

[ApiController]
[Route("debug")]
public sealed class DebugController(
    IPollingOrchestrator pollingOrchestrator,
    IEventDetectionService eventDetectionService) : ControllerBase
{
    [HttpPost("trigger-check")]
    public async Task<IActionResult> TriggerCheck(CancellationToken cancellationToken)
    {
        await pollingOrchestrator.PollAllAsync(cancellationToken);
        return Accepted();
    }

    [HttpPost("rules/temperature/trigger")]
    public async Task<IActionResult> TriggerTemperatureRule(
        [FromQuery] Guid? aquariumId,
        CancellationToken cancellationToken)
    {
        var data = CreateBaseData(aquariumId, temperatureCelsius: 30.0m);
        await eventDetectionService.DetectAndPublishAsync(data, cancellationToken);

        return Accepted(new
        {
            aquariumId = data.AquariumId,
            triggeredRule = "temperature-threshold"
        });
    }

    [HttpPost("rules/water-quality/trigger")]
    public async Task<IActionResult> TriggerWaterQualityRule(
        [FromQuery] Guid? aquariumId,
        CancellationToken cancellationToken)
    {
        var data = CreateBaseData(aquariumId, turbidity: 8.0m);
        await eventDetectionService.DetectAndPublishAsync(data, cancellationToken);

        return Accepted(new
        {
            aquariumId = data.AquariumId,
            triggeredRule = "water-quality"
        });
    }

    private static AquariumData CreateBaseData(
        Guid? aquariumId,
        decimal temperatureCelsius = 25.0m,
        decimal ph = 7.2m,
        decimal turbidity = 2.0m) => new()
    {
        AquariumId = aquariumId ?? Guid.NewGuid(),
        TemperatureCelsius = temperatureCelsius,
        Ph = ph,
        Turbidity = turbidity,
        Mg = 1200m,
        Kh = 7.0m,
        Ca = 420m,
        Oxygen = 8.0m,
        Pump = 1.0m,
        CapturedAt = DateTimeOffset.UtcNow
    };
}
