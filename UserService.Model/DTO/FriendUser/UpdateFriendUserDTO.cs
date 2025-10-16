using System.ComponentModel.DataAnnotations;
using UserService.Model.Enums;
using UserService.Model.Utilities;

namespace UserService.Model.DTO.FriendUser;

public record UpdateFriendUserDTO(Guid UserId, Guid FriendId, [EnumDescription(typeof(FriendStatus))] string Status);