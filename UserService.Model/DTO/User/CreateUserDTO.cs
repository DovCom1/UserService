using System.ComponentModel.DataAnnotations;
using UserService.Model.Enums;

namespace UserService.Model.DTO.User;

public record CreateUserDTO(
    [Required] [StringLength(10, MinimumLength = 1, ErrorMessage = "UID должен содержать не менее 1 и не более 10 символов")]
    string Uid,
    [Required] [StringLength(50, MinimumLength = 1, ErrorMessage = "Никнейм должен содержать не менее 1 и не более 50 символов")]
    string Nickname,
    [Required] [EmailAddress(ErrorMessage = "Неверный адрес электронной почты")]
    string Email,
    string Gender,
    [Required] DateOnly DateOfBirth
    );