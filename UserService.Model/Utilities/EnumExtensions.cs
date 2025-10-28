using System.ComponentModel;
using System.Reflection;

namespace UserService.Model.Utilities;

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
    
    // В случае подключения Kafka/RabbitMQ нужно делать более безопасным
    public static T ParseByDescription<T>(this string description) where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .First(e => e.GetDescription() == description);
    }
}