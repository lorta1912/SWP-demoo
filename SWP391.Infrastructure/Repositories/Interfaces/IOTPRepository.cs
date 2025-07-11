using SWP391.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Infrastructure.Repositories.Interfaces
{
    public interface IOTPRepository
    {
        Task AddOTPAsync(OTP otp);
        Task<OTP?> GetOTPByEmailAndCodeAsync(string email, string code);
        Task UpdateOTPAsync(OTP otp);
    }
}
