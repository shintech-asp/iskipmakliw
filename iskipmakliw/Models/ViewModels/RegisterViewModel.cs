using System.ComponentModel.DataAnnotations;

namespace iskipmakliw.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class VerifyEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Code must be 4 digits.")]
        [RegularExpression("^[0-9]{4}$", ErrorMessage = "Code must be 4 digits.")]
        public string Code { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
