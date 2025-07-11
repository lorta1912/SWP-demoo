using Microsoft.EntityFrameworkCore; // Thêm dòng này
using SWP391.Infrastructure;
using SWP391.Infrastructure.Data;
using SWP391.Infrastructure.Entities;
using SWP391.Infrastructure.Repositories.Interfaces;

namespace SWP391.Infrastructure.Repositories
{
    public class OTPRepository : IOTPRepository
    {
        private readonly AppDbContext _context;

        public OTPRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddOTPAsync(OTP otp)
        {
            await _context.OTPs.AddAsync(otp);
            await _context.SaveChangesAsync();
        }

        public async Task<OTP?> GetOTPByEmailAndCodeAsync(string email, string code)
        {
            return await _context.OTPs
                .FirstOrDefaultAsync(o => o.Email.ToLower() == email.ToLower() && o.Code == code && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);
        }

        public async Task UpdateOTPAsync(OTP otp)
        {
            _context.OTPs.Update(otp);
            await _context.SaveChangesAsync();
        }
    }
}