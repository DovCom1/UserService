namespace UserService.Model.DTO.User;

public record PagedUsersDTO( IEnumerable<UserDTO> Data,
    int Offset,
    int Limit,
    int Total);