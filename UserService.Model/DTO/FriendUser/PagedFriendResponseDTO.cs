using UserService.Model.DTO.User;

namespace UserService.Model.DTO.FriendUser;

public record PagedFriendResponseDTO(
    IEnumerable<ShortUserDTO> Data,
    int Offset,
    int Limit,
    int Total);