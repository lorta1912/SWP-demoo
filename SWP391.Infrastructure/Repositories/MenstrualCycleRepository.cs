using Microsoft.EntityFrameworkCore;
using SWP391.Infrastructure.Data;
using SWP391.Infrastructure.Entities;
using SWP391.Infrastructure.Repositories.Interfaces;

namespace SWP391.Infrastructure.Repositories
{
    public class MenstrualCycleRepository : IMenstrualCycleRepository
    {
        private readonly AppDbContext _context;

        public MenstrualCycleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenstrualCycle>> GetAllAsync()
        {
            return await _context.MenstrualCycles.Include(x => x.User).ToListAsync();
        }

        public async Task<MenstrualCycle?> GetByIdAsync(int id)
        {
            return await _context.MenstrualCycles.Include(x => x.User)
                                                  .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<MenstrualCycle>> GetByUserIdAsync(int userId)
        {
            return await _context.MenstrualCycles.Include(x => x.User)
                                                  .Where(x => x.UserId == userId)
                                                  .ToListAsync();
        }

        public async Task AddMenstrualCycleAsync(MenstrualCycle cycle)
        {
            _context.MenstrualCycles.Add(cycle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMenstrualCycleAsync(MenstrualCycle cycle)
        {
            _context.MenstrualCycles.Update(cycle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMenstrualCycleAsync(MenstrualCycle cycle)
        {
            _context.MenstrualCycles.Remove(cycle);
            await _context.SaveChangesAsync();
        }

        public async Task<MenstrualCycle?> GetMenstrualCycleByUserIdThatIncludeThatDay(int UserId, DateOnly Date)
        {
            return await _context.MenstrualCycles
                .FirstOrDefaultAsync(c => c.UserId == UserId && Date >= c.StartDate && Date <= c.EndDate);
        }

        public async Task<MenstrualCycle?> GetMenstrualCycleByUserIdThatThatDayIsAfterEndDate(int UserId, DateOnly Date)
        {
            return await _context.MenstrualCycles
                .FirstOrDefaultAsync(c => c.UserId == UserId && c.EndDate == Date.AddDays(-1));
        }

        public async Task<MenstrualCycle?> GetMenstrualCycleByUserIdThatThatDayIsBeforeStartDate(int UserId, DateOnly Date)
        {
            return await _context.MenstrualCycles
                .FirstOrDefaultAsync(c => c.UserId == UserId && c.StartDate == Date.AddDays(1));
        }

        public async Task<MenstrualCycle?> TakeLastCycleByUserIdAsync(int UserId)
        {
            return await _context.MenstrualCycles
                .Where(x => x.UserId == UserId)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MenstrualCycle>> TakeCyclesByMonthAsync(int UserId, int month, int year)
        {
            return await _context.MenstrualCycles
                .Where(mc => mc.UserId == UserId &&
                    (
                        (mc.StartDate.Month == month && mc.StartDate.Year == year) ||
                        (mc.EndDate.Month == month && mc.EndDate.Year == year)
                    )
                )
                .OrderBy(mc => mc.StartDate) 
                .ToListAsync();
        }
    }
}
