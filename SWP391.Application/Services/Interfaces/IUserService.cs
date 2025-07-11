using SWP391.Application.DTOs;

namespace SWP391.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(RegisterDto registerDto);
        Task<string> LoginAsync(LoginDto loginDto);
        Task SendOTPEmailAsync(string email);
        Task VerifyOTPAsync(VerifyOTPDto verifyOTPDto);
        Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}