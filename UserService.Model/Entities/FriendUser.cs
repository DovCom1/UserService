namespace UserService.Model.Entities;

public class FriendUser
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid FriendId { get; set; }
    public User Friend { get; set; }
    public short Status { get; set; }
}