using System.ComponentModel;

namespace UserService.Model.Enums;

public enum FriendStatus
{
    [Description("Заявка отправлена")]
    ApplicationSent = 1,
    
    [Description("Друг")]
    Friend = 2,
    
    [Description("Заявка отклонена")]
    ApplicationRejected = 3
}