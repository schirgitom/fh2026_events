namespace AutomationService.Infrastructure.Configuration;

public sealed class GraphQlOptions
{
    public const string SectionName = "GraphQl";
    public string Endpoint { get; init; } = "http://localhost:8080/graphql";
}
