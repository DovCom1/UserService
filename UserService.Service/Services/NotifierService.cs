using Microsoft.Extensions.Logging;
using UserService.Contract.Services;
using UserService.Model.DTO.Notify;
using UserService.Service.Request;

namespace UserService.Service.Services;

public class NotifierService(IHttpClientFactory httpClientFactory, RequestFactory requestFactory, ILogger<NotifierService> _logger) : INotifierService
{
    public async Task NotifySendFriendRequestAsync(FriendRequestDTO body, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient();
        var request = requestFactory.NotifySendFriendRequest(body, ct);
        var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to send friend request notify: {response.StatusCode}");
        }
    }
}