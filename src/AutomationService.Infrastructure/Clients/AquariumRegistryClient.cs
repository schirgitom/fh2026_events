using System.Text.Json;
using AutomationService.Application.Abstractions;
using AutomationService.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutomationService.Infrastructure.Clients;

public sealed class AquariumRegistryClient(
    HttpClient httpClient,
    IOptions<AquariumRegistryOptions> options,
    ILogger<AquariumRegistryClient> logger) : IAquariumRegistryClient
{
    public async Task<IReadOnlyCollection<Guid>> GetAquariumIdsAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuthHeader();

        var freshwaterIds = await FetchIdsAsync(options.Value.FreshWaterPath, cancellationToken);
        var seawaterIds = await FetchIdsAsync(options.Value.SeaWaterPath, cancellationToken);

        var allIds = freshwaterIds
            .Concat(seawaterIds)
            .Distinct()
            .ToArray();

        logger.LogInformation(
            "Loaded {AquariumCount} unique aquarium IDs from registry: {AquariumIds}",
            allIds.Length,
            allIds);

        return allIds;
    }

    private async Task<IReadOnlyCollection<Guid>> FetchIdsAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        using var document = JsonDocument.Parse(content);
        var ids = new HashSet<Guid>();

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            ExtractFromArray(document.RootElement, ids);
        }
        else if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (document.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                ExtractFromArray(items, ids);
            }
            else if (document.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                ExtractFromArray(data, ids);
            }
        }

        var idsArray = ids.ToArray();
        logger.LogInformation(
            "Loaded {AquariumCount} aquarium IDs from registry path {RegistryPath}: {AquariumIds}",
            idsArray.Length,
            path,
            idsArray);

        return idsArray;
    }

    private void ExtractFromArray(JsonElement items, ISet<Guid> ids)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (!TryReadId(item, out var aquariumId))
            {
                continue;
            }

            ids.Add(aquariumId);
        }
    }

    private bool TryReadId(JsonElement item, out Guid aquariumId)
    {
        foreach (var property in item.EnumerateObject())
        {
            if (!string.Equals(property.Name, "id", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.String &&
                Guid.TryParse(property.Value.GetString(), out aquariumId))
            {
                return true;
            }
        }

        aquariumId = Guid.Empty;
        return false;
    }

    private void ApplyAuthHeader()
    {
        var headerName = options.Value.ServiceAuth.HeaderName;
        var apiKey = options.Value.ServiceAuth.ApiKey;

        if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(apiKey))
        {
            return;
        }

        httpClient.DefaultRequestHeaders.Remove(headerName);
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(headerName, apiKey);
    }
}
