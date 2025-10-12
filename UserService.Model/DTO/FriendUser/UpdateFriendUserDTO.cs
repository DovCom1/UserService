using System.ComponentModel.DataAnnotations;
using UserService.Model.Enums;

namespace UserService.Model.DTO.FriendUser;

public record UpdateFriendUserDTO(
    [Required] [EnumDataType(typeof(FriendStatus), ErrorMessage = "Неверный статус дружбы")]
    FriendStatus status);