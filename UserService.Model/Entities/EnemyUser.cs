namespace UserService.Model.Entities;

public class EnemyUser
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid EnemyId { get; set; }
    public User Enemy  { get; set; }

    private EnemyUser() { }

    public EnemyUser(Guid userId, Guid enemyId)
    {
        UserId = userId;
        EnemyId = enemyId;
    }
}