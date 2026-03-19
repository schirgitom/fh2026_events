using Microsoft.AspNetCore.SignalR;

namespace AutomationService.Api.Hubs;

public sealed class NotificationHub : Hub
{
    private const string AllAquariumsGroup = "aquarium:all";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AllAquariumsGroup);
        await base.OnConnectedAsync();
    }

    public Task SubscribeToAquarium(Guid aquariumId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GetAquariumGroup(aquariumId));

    public Task UnsubscribeFromAquarium(Guid aquariumId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GetAquariumGroup(aquariumId));

    public Task SubscribeToAll() =>
        Groups.AddToGroupAsync(Context.ConnectionId, AllAquariumsGroup);

    public Task UnsubscribeFromAll() =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, AllAquariumsGroup);

    public string GetAquariumGroup(Guid aquariumId) => $"aquarium:{aquariumId}";

    public static string GetAllAquariumsGroup() => AllAquariumsGroup;
}
