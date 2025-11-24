using UserService.Model.Enums;

namespace UserService.Model.Entities;

public class FriendUser
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid FriendId { get; set; }
    public User Friend { get; set; }
    public FriendStatus Status { get; set; }

    private FriendUser() { }

    public FriendUser(Guid userId, Guid friendId, FriendStatus status)
    {
        UserId = userId;
        FriendId = friendId;
        Status = status;
    }

    public FriendUser(Guid userId, Guid friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}