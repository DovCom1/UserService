using System.ComponentModel.DataAnnotations;
using UserService.Model.Enums;
using UserService.Model.Utilities;

namespace UserService.Model.DTO.FriendUser;

public record UpdateFriendUserDTO(
    Guid UserId,
    Guid FriendId,
    [Required(ErrorMessage = "Поле Статус обязательно для заполнения")]
    [EnumDescription(typeof(FriendStatus))]
    string Status
    );