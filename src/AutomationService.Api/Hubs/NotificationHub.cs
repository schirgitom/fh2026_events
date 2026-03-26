using AutomationService.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AutomationService.Api.Hubs;

public sealed class NotificationHub(
    IFeedingService feedingService,
    ILogger<NotificationHub> logger) : Hub
{
    private const string AllAquariumsGroup = "aquarium:all";

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("SignalR client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async Task SubscribeToAquarium(Guid aquariumId)
    {
        var group = GetAquariumGroup(aquariumId);
        logger.LogInformation(
            "SignalR client {ConnectionId} subscribing to group {Group}.",
            Context.ConnectionId,
            group);

        await Groups.AddToGroupAsync(Context.ConnectionId, group);

        var status = await feedingService.GetCurrentStatusAsync(aquariumId, Context.ConnectionAborted);
        var payload = new
        {
            aquariumId = status.AquariumId,
            lastFeedingAt = status.LastFeedingAt,
            nextFeedingAt = status.NextFeedingAt,
            remainingSeconds = status.RemainingSeconds,
            isOverdue = status.IsOverdue,
            message = (string?)null
        };

        await Clients.Caller.SendAsync("feeding:update", payload, Context.ConnectionAborted);
    }

    public Task UnsubscribeFromAquarium(Guid aquariumId)
    {
        var group = GetAquariumGroup(aquariumId);
        logger.LogInformation(
            "SignalR client {ConnectionId} unsubscribing from group {Group}.",
            Context.ConnectionId,
            group);

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    }

    public Task SubscribeToAll()
    {
        logger.LogInformation(
            "SignalR client {ConnectionId} subscribing to group {Group}.",
            Context.ConnectionId,
            AllAquariumsGroup);

        return Groups.AddToGroupAsync(Context.ConnectionId, AllAquariumsGroup);
    }

    public Task UnsubscribeFromAll()
    {
        logger.LogInformation(
            "SignalR client {ConnectionId} unsubscribing from group {Group}.",
            Context.ConnectionId,
            AllAquariumsGroup);

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, AllAquariumsGroup);
    }

    public string GetAquariumGroup(Guid aquariumId) => $"aquarium:{aquariumId}";

    public static string GetAllAquariumsGroup() => AllAquariumsGroup;
}
