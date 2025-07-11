using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Api.Services.Interfaces
{
    public interface ICurrentUserService
    {
        Task<int> GetCurrentUserIdAsync();
    }
}

