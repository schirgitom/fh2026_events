using AutomationService.Application.Abstractions;
using AutomationService.Domain.Models;

namespace AutomationService.Application.Rules;

public sealed class TemperatureRule : IRule<AquariumData>
{
    private const decimal MaxTemperature = 28.0m;
    private const decimal MinTemperature = 20.0m;

    public string Name => "temperature-threshold";
    public string Description => "Temperature out of safe aquarium range.";

    public bool IsSatisfied(AquariumData input) =>
        input.TemperatureCelsius < MinTemperature || input.TemperatureCelsius > MaxTemperature;
}
