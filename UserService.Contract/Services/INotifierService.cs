using UserService.Model.DTO.Notify;

namespace UserService.Contract.Services;

public interface INotifierService
{
    public Task NotifySendFriendRequestAsync(FriendRequestDTO body, CancellationToken ct = default);
}