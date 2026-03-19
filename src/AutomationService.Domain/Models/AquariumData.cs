namespace AutomationService.Domain.Models;

public sealed class AquariumData
{
    public Guid AquariumId { get; init; }
    public decimal TemperatureCelsius { get; init; }
    public decimal Ph { get; init; }
    public decimal Turbidity { get; init; }
    public decimal Mg { get; init; }
    public decimal Kh { get; init; }
    public decimal Ca { get; init; }
    public decimal Oxygen { get; init; }
    public decimal Pump { get; init; }
    public DateTimeOffset CapturedAt { get; init; }
}
