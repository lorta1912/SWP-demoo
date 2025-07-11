using SWP391.Infrastructure.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWP391.Infrastructure.Repositories.Interfaces
{
    public interface IPillIntakeCycleRepository
    {
        Task<IEnumerable<PillIntakeCycle>> GetAllAsync();
        Task<PillIntakeCycle?> GetByIdAsync(int id);
        Task<IEnumerable<PillIntakeCycle>> GetByUserIdAsync(int userId);
        Task AddAsync(PillIntakeCycle cycle);
        Task UpdateAsync(PillIntakeCycle cycle);
        Task DeleteAsync(int id);

        Task<List<PillIntakeCycle>> GetOngoingPillIntakeCycleAsync(DateOnly today);
        Task<bool> IsAnyOnGoingPillIntakeCycleByUserIdAsync(int UserId, DateOnly today);
        Task<List<PillIntakeCycle>> GetExpiringPillIntakeCycleAsync(DateOnly today);
        Task<List<PillIntakeCycle>> GetAllPackRelatedToThisMonth(int userId,int month, int year);
    }
}
