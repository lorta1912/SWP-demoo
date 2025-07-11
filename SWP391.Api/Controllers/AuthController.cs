using Microsoft.AspNetCore.Mvc;
using SWP391.Api.Models;
using SWP391.Application.DTOs;
using SWP391.Application.Services.Interfaces;

namespace SWP391.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                await _userService.RegisterAsync(registerDto);
                return Ok(ApiResponse<object>.Success(null, "Registration successful. Please verify your email."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(400, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Error(409, ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = await _userService.LoginAsync(loginDto);
                return Ok(ApiResponse<string>.Success(token, "Login successful."));
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(ApiResponse<object>.Error(401, ex.Message));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                await _userService.SendOTPEmailAsync(forgotPasswordDto.Email);
                return Ok(ApiResponse<object>.Success(null, "OTP sent to your email."));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.Error(404, ex.Message));
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPDto verifyOTPDto)
        {
            try
            {
                await _userService.VerifyOTPAsync(verifyOTPDto);
                return Ok(ApiResponse<object>.Success(null, "OTP verified successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(400, ex.Message));
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                await _userService.ResetPasswordAsync(resetPasswordDto);
                return Ok(ApiResponse<object>.Success(null, "Password reset successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(400, ex.Message));
            }
        }
    }
}