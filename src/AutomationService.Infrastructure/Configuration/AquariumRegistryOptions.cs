namespace AutomationService.Infrastructure.Configuration;

public sealed class AquariumRegistryOptions
{
    public const string SectionName = "AquariumRegistry";
    public string BaseUrl { get; init; } = "http://localhost:7214";
    public string FreshWaterPath { get; init; } = "/api/FreshWaterAquarium";
    public string SeaWaterPath { get; init; } = "/api/SeaWaterAquarium";
    public ServiceAuthOptions ServiceAuth { get; init; } = new();
}

public sealed class ServiceAuthOptions
{
    public string HeaderName { get; init; } = "X-Service-Key";
    public string ApiKey { get; init; } = string.Empty;
}
