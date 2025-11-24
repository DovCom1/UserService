using System.ComponentModel.DataAnnotations;
using UserService.Model.Enums;
using UserService.Model.Utilities;

namespace UserService.Model.DTO.User;

public record CreateUserDTO(
    [Required(ErrorMessage = "Поле UID обязательно для заполнения")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "UID должен содержать не менее 1 и не более 10 символов")]
    string Uid,
    
    [Required(ErrorMessage = "Поле Никнейм обязательно для заполнения")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Никнейм должен содержать не менее 1 и не более 50 символов")]
    string Nickname,
    
    [Required(ErrorMessage = "Поле Email обязательно для заполнения")]
    [EmailAddress(ErrorMessage = "Неверный адрес электронной почты")]
    string Email,
    
    [Required(ErrorMessage = "Поле Пол обязательно для заполнения")]
    [EnumDescription(typeof(Gender))]
    string Gender,
    
    [Required(ErrorMessage = "Дата рождения обязательна для заполнения")]
    DateOnly DateOfBirth
    );