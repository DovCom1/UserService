using System.ComponentModel;
using System.Reflection;

namespace UserService.Api.Utilities;

public static class EnumExtensions
{
    public static string GetDescription<T>(this T value) where T : Enum
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo == null) return value.ToString();
        var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description ?? value.ToString();
    }
}