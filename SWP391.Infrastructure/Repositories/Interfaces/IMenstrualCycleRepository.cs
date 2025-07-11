using SWP391.Infrastructure.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWP391.Infrastructure.Repositories.Interfaces
{
    public interface IMenstrualCycleRepository
    {
        Task<IEnumerable<MenstrualCycle>> GetAllAsync();
        Task<MenstrualCycle?> GetByIdAsync(int id);
        Task<IEnumerable<MenstrualCycle>> GetByUserIdAsync(int userId);
        Task AddMenstrualCycleAsync(MenstrualCycle cycle);
        Task UpdateMenstrualCycleAsync(MenstrualCycle cycle);
        Task DeleteMenstrualCycleAsync(MenstrualCycle cycle);

        Task<MenstrualCycle?> GetMenstrualCycleByUserIdThatIncludeThatDay(int UserId, DateOnly Date);
        Task<MenstrualCycle?> GetMenstrualCycleByUserIdThatThatDayIsAfterEndDate(int UserId, DateOnly Date);
        Task<MenstrualCycle?> GetMenstrualCycleByUserIdThatThatDayIsBeforeStartDate(int UserId, DateOnly Date);

        Task<MenstrualCycle?> TakeLastCycleByUserIdAsync(int UserId);

        Task<List<MenstrualCycle>> TakeCyclesByMonthAsync(int UserId, int month, int year);
    }
}