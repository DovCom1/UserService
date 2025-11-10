using UserService.Model.Enums;

namespace UserService.Model.DTO.User;

public record UserDTO(
    Guid Id,
    string Uid,
    string Nickname,
    string Email,
    string AvatarUrl,
    string Gender,
    string Status,
    DateOnly DateOfBirth,
    DateOnly AccountCreationTime
    );