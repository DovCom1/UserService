using UserService.Model.Enums;

namespace UserService.Model.DTO.User;

public record ShortUserDTO(
    Guid Id,
    string Uid,
    string Nickname,
    string AvatarUrl,
    string Status
);