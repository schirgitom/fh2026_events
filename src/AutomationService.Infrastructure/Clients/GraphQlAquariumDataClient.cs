using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutomationService.Application.Abstractions;
using AutomationService.Domain.Models;
using Microsoft.Extensions.Logging;

namespace AutomationService.Infrastructure.Clients;

public sealed class GraphQlAquariumDataClient(
    HttpClient httpClient,
    ILogger<GraphQlAquariumDataClient> logger) : IAquariumDataClient
{
    public async Task<AquariumData> GetAquariumDataAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new
            {
                query = """
                        query ($aquariumId: ID!) {
                          latestMeasurement(aquariumId: $aquariumId) {
                            aquariumId
                            timestamp
                            temperature
                            mg
                            kh
                            ca
                            ph
                            oxygen
                            pump
                          }
                        }
                        """,
                variables = new { aquariumId }
            };

            var response = await httpClient.PostAsJsonAsync(string.Empty, query, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            GraphQlResponse result;
            try
            {
                result = JsonSerializer.Deserialize<GraphQlResponse>(responseContent)
                         ?? throw new InvalidOperationException("GraphQL response was empty.");
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to decode GraphQL response for aquarium ID: {AquariumId}. Response: {ResponseContent}",
                    aquariumId,
                    responseContent);
                throw;
            }

            var measurement = result.Data?.LatestMeasurement
                              ?? throw new InvalidOperationException("GraphQL payload missing latestMeasurement data.");

            return new AquariumData
            {
                AquariumId = measurement.AquariumId,
                TemperatureCelsius = measurement.Temperature,
                Ph = measurement.Ph,
                Turbidity = 0m,
                Mg = measurement.Mg,
                Kh = measurement.Kh,
                Ca = measurement.Ca,
                Oxygen = measurement.Oxygen ?? 0m,
                Pump = measurement.Pump ?? 0m,
                CapturedAt = measurement.Timestamp
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch aquarium data for aquarium ID: {AquariumId}", aquariumId);
            throw;
        }
    }

    private sealed class GraphQlResponse
    {
        [JsonPropertyName("data")]
        public GraphQlData? Data { get; init; }
    }

    private sealed class GraphQlData
    {
        [JsonPropertyName("latestMeasurement")]
        public LatestMeasurementPayload? LatestMeasurement { get; init; }
    }

    private sealed class LatestMeasurementPayload
    {
        [JsonPropertyName("aquariumId")]
        public Guid AquariumId { get; init; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; init; }

        [JsonPropertyName("temperature")]
        public decimal Temperature { get; init; }

        [JsonPropertyName("mg")]
        public decimal Mg { get; init; }

        [JsonPropertyName("kh")]
        public decimal Kh { get; init; }

        [JsonPropertyName("ca")]
        public decimal Ca { get; init; }

        [JsonPropertyName("ph")]
        public decimal Ph { get; init; }

        [JsonPropertyName("oxygen")]
        public decimal? Oxygen { get; init; }

        [JsonPropertyName("pump")]
        public decimal? Pump { get; init; }
    }
}
