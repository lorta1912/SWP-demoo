using System.ComponentModel.DataAnnotations;

namespace SWP391.Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = null!;

        public DateOnly? DateOfBirth { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = null!;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = null!;
    }

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;
    }

    public class VerifyOTPDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required]
        public string OTP { get; set; } = null!;
    }

    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required]
        public string OTP { get; set; } = null!;

        [Required]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}