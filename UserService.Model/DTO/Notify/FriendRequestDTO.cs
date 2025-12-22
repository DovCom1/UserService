namespace UserService.Model.DTO.Notify;

public record FriendRequestDTO(Guid SenderId, Guid ReceiverId, string SenderName, string ReceiverName, DateTime createdAt, string TypeDto = "Invite");