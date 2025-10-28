using System.ComponentModel;

namespace UserService.Model.Enums;

public enum UserStatus
{
    [Description("В сети")]
    Online = 1,
    
    [Description("Не активен")]
    Inactive = 2,
    
    [Description("Не беспокоить")]
    DoNotDisturb = 3,
    
    [Description("Не в сети")]
    Offline = 4
}