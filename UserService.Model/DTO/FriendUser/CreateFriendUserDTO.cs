using System.ComponentModel.DataAnnotations;

namespace UserService.Model.DTO.FriendUser;

public record CreateFriendUserDTO([Required] Guid UserId, [Required] Guid FriendId);