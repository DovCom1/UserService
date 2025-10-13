using System.ComponentModel.DataAnnotations;

namespace UserService.Model.Utilities;

public class EnumDescriptionAttribute(Type enumType) : ValidationAttribute
{
    private readonly Type _enumType = enumType;

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success!;
        
        var strValue = value.ToString();
        if (string.IsNullOrEmpty(strValue)) 
            return new ValidationResult($"Значение для {validationContext.DisplayName} не может быть пустым.");
        
        var descriptions = Enum.GetValues(_enumType)
            .Cast<Enum>()
            .Select(e => e.GetDescription())
            .ToList();

        if (!descriptions.Contains(strValue))
            return new ValidationResult($"Значение '{strValue}' для {validationContext.DisplayName} недопустимо. Допустимые значения: {string.Join(", ", descriptions)}.");

        return ValidationResult.Success!; 
    }
}