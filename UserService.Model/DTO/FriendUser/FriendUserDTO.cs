namespace UserService.Model.DTO.FriendUser;

public record FriendUserDTO(Guid UserId, Guid FriendId, string Status);