using UserService.Model.DTO.User;

namespace UserService.Model.DTO.EnemyUser;

public record PagedEnemyResponseDTO(
    IEnumerable<ShortUserDTO> Data,
    int Offset,
    int Limit,
    int Total);