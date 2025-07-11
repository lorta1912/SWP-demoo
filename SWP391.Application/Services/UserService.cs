using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SWP391.Application.DTOs;
using SWP391.Application.Services.Interfaces;
using SWP391.Infrastructure;
using SWP391.Infrastructure.Data;
using SWP391.Infrastructure.Entities;
using SWP391.Infrastructure.Enums;
using SWP391.Infrastructure.Repositories.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOTPRepository _otpRepository;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context; // Thêm AppDbContext
        private readonly string _jwtSecret;

        public UserService(
            IUserRepository userRepository,
            IOTPRepository otpRepository,
            IEmailService emailService,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _otpRepository = otpRepository ?? throw new ArgumentNullException(nameof(otpRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtSecret = configuration["Jwt:Key"] ?? throw new ArgumentNullException(nameof(configuration), "Jwt:Key is not configured.");
        }

        public async Task<string> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                throw new ArgumentException("Invalid email or password.");
            }

            if (!user.IsEmailVerified)
            {
                throw new ArgumentException("Email not verified. Please verify your email with OTP.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("role", user.Role.ToString())
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match.");
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    FullName = registerDto.FullName,
                    DateOfBirth = registerDto.DateOfBirth,
                    Email = registerDto.Email.ToLower(),
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Address = registerDto.Address,
                    PhoneNumber = registerDto.PhoneNumber,
                    Role = RoleEnum.Customer,
                    IsEmailVerified = false // Mặc định chưa xác thực
                };

                await _userRepository.AddUserAsync(user);

                var otpCode = new Random().Next(100000, 999999).ToString();
                var otp = new OTP
                {
                    Email = registerDto.Email.ToLower(),
                    Code = otpCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false
                };

                await _otpRepository.AddOTPAsync(otp);

                var subject = "Your OTP Code";
                var body = $"<h3>Your OTP code is: {otpCode}</h3><p>This code is valid for 10 minutes.</p>";
                await _emailService.SendEmailAsync(registerDto.Email, subject, body);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SendOTPEmailAsync(string email)
        {
            var otpCode = new Random().Next(100000, 999999).ToString();
            var otp = new OTP
            {
                Email = email.ToLower(),
                Code = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _otpRepository.AddOTPAsync(otp);

            var subject = "Your OTP Code";
            var body = $"<h3>Your OTP code is: {otpCode}</h3><p>This code is valid for 10 minutes.</p>";
            await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task VerifyOTPAsync(VerifyOTPDto verifyOTPDto)
        {
            var otp = await _otpRepository.GetOTPByEmailAndCodeAsync(verifyOTPDto.Email, verifyOTPDto.OTP);
            if (otp == null)
            {
                throw new ArgumentException("Invalid or expired OTP.");
            }

            var user = await _userRepository.GetUserByEmailAsync(verifyOTPDto.Email);
            if (user == null)
            {
                throw new ArgumentException("Email not found.");
            }

            otp.IsUsed = true;
            user.IsEmailVerified = true; // Cập nhật trạng thái xác thực
            await _otpRepository.UpdateOTPAsync(otp);
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match.");
            }

            var otp = await _otpRepository.GetOTPByEmailAndCodeAsync(resetPasswordDto.Email, resetPasswordDto.OTP);
            if (otp == null || otp.IsUsed)
            {
                throw new ArgumentException("Invalid or expired OTP.");
            }

            var user = await _userRepository.GetUserByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                throw new ArgumentException("Email not found.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            await _userRepository.UpdateUserAsync(user);

            otp.IsUsed = true;
            await _otpRepository.UpdateOTPAsync(otp);
        }
    }
}