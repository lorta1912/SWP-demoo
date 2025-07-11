using Microsoft.EntityFrameworkCore;
using SWP391.Infrastructure.Data;
using SWP391.Infrastructure.Entities;
using SWP391.Infrastructure.Repositories.Interfaces;

namespace SWP391.Infrastructure.Repositories
{
    public class PillIntakeCycleRepository : IPillIntakeCycleRepository
    {
        private readonly AppDbContext _context;

        public PillIntakeCycleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PillIntakeCycle>> GetAllAsync()
        {
            return await _context.PillIntakeCycles.Include(x => x.User).ToListAsync();
        }

        public async Task<PillIntakeCycle?> GetByIdAsync(int id)
        {
            return await _context.PillIntakeCycles.Include(x => x.User)
                                                   .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PillIntakeCycle>> GetByUserIdAsync(int userId)
        {
            return await _context.PillIntakeCycles.Include(x => x.User)
                                                   .Where(x => x.UserId == userId)
                                                   .ToListAsync();
        }

        public async Task AddAsync(PillIntakeCycle cycle)
        {
            var newStart = cycle.StartDate;
            var newEnd = cycle.StartDate.AddDays(cycle.PackSize - 1);

            var hasOverlap = await _context.PillIntakeCycles
                .AnyAsync(p =>
                    p.UserId == cycle.UserId &&
                    // Khoảng này giao nhau
                    newStart <= p.StartDate.AddDays(p.PackSize - 1) &&
                    newEnd >= p.StartDate
                );

            if (hasOverlap)
            {
                throw new InvalidOperationException("Cycle bị trùng với 1 chu kỳ khác của user này.");
            }

            _context.PillIntakeCycles.Add(cycle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PillIntakeCycle cycle)
        {
            _context.PillIntakeCycles.Update(cycle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.PillIntakeCycles.FindAsync(id);
            if (entity != null)
            {
                _context.PillIntakeCycles.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PillIntakeCycle>> GetOngoingPillIntakeCycleAsync(DateOnly today)
        {            
            return await _context.PillIntakeCycles
                .Where(p => today >= p.StartDate && today <= p.StartDate.AddDays(p.PackSize - 1))
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<bool> IsAnyOnGoingPillIntakeCycleByUserIdAsync(int UserId,DateOnly today)
        {
            var ongoing = await _context.PillIntakeCycles
                .Where(p => today >= p.StartDate && today <= p.StartDate.AddDays(p.PackSize - 1) && p.UserId == UserId)
                .FirstOrDefaultAsync();

            if (ongoing != null) return true;
            else return false;
        }

        public async Task<List<PillIntakeCycle>> GetExpiringPillIntakeCycleAsync(DateOnly today)
        {
            return await _context.PillIntakeCycles
                .Where(p => today == p.StartDate.AddDays(p.PackSize - 3) )
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<List<PillIntakeCycle>> GetAllPackRelatedToThisMonth(int userId, int month, int year)
        {
            var startOfMonth = new DateOnly(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await _context.PillIntakeCycles
                .Where(p => p.UserId == userId)
                .Where(p =>
                    (p.StartDate >= startOfMonth && p.StartDate <= endOfMonth) ||
                    (p.StartDate.AddDays(p.PackSize - 1) >= startOfMonth && p.StartDate.AddDays(p.PackSize - 1) <= endOfMonth)
                )
                .ToListAsync();
        }
    }
}
