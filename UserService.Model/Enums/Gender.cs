using System.ComponentModel;

namespace UserService.Model.Enums;

public enum Gender
{
    [Description("Мужской")]
    Male = 1,
    
    [Description("Женский")]
    Female = 2
}