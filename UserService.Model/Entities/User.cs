using UserService.Model.Enums;

namespace UserService.Model.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Uid { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string AvatarUrl { get; set; }
    public Gender Gender { get; set; }
    public UserStatus Status { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public DateTime AccountCreationTime { get; set; }
    public ICollection<FriendUser> Friends { get; set; }
    public ICollection<EnemyUser> Enemies { get; set; }
}