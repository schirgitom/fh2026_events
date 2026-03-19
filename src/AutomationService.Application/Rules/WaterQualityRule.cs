using AutomationService.Application.Abstractions;
using AutomationService.Domain.Models;

namespace AutomationService.Application.Rules;

public sealed class WaterQualityRule : IRule<AquariumData>
{
    private const decimal MinPh = 6.5m;
    private const decimal MaxPh = 8.5m;
    private const decimal MaxTurbidity = 5.0m;

    public string Name => "water-quality";
    public string Description => "Water quality outside configured pH or turbidity limits.";

    public bool IsSatisfied(AquariumData input) =>
        input.Ph < MinPh || input.Ph > MaxPh || input.Turbidity > MaxTurbidity;
}
