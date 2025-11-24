using System.ComponentModel.DataAnnotations;

namespace UserService.Model.DTO.EnemyUser;

public record CreateEnemyUserDTO([Required] Guid UserId, [Required] Guid EnemyId);