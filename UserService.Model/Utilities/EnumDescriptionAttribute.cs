using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Utilities;

public class EnumDescriptionAttribute(Type enumType) : ValidationAttribute
{
    private static readonly ConcurrentDictionary<Type, List<string>> Cache = new();

    protected override ValidationResult IsValid(object? enumDescriptionValue, ValidationContext validationContext)
    {
        if (enumDescriptionValue == null) return ValidationResult.Success!;
        
        var strValue = enumDescriptionValue.ToString();
        if (string.IsNullOrEmpty(strValue)) 
            return new ValidationResult($"Значение для {validationContext.DisplayName} не может быть пустым.");
        
        var descriptions = Cache.GetOrAdd(enumType, type => 
            Enum.GetValues(enumType)
            .Cast<Enum>()
            .Select(e => e.GetDescription())
            .ToList());

        if (!descriptions.Contains(strValue))
            return new ValidationResult($"Значение '{strValue}' для {validationContext.DisplayName} недопустимо. Допустимые значения: {string.Join(", ", descriptions)}.");

        return ValidationResult.Success!; 
    }
}