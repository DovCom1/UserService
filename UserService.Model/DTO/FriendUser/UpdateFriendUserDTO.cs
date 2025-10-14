using System.ComponentModel.DataAnnotations;
using UserService.Model.Enums;
using UserService.Model.Utilities;

namespace UserService.Model.DTO.FriendUser;

public record UpdateFriendUserDTO([EnumDescription(typeof(FriendStatus))] string Status);