using SWP391.Application.DTOs;
using SWP391.Application.Services.Interfaces;
using SWP391.Infrastructure.Data;
using SWP391.Infrastructure.Entities;
using SWP391.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Application.Services
{
    public class PillIntakeCycleService : IPillIntakeCycleService
    {
        private readonly AppDbContext _context;
        
        private readonly IPillIntakeCycleRepository _pillIntakeCycleRepository;

        public PillIntakeCycleService(AppDbContext context, IPillIntakeCycleRepository pillIntakeCycleRepository)
        {          
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _pillIntakeCycleRepository = pillIntakeCycleRepository ?? throw new ArgumentNullException(nameof(pillIntakeCycleRepository));
        }
        public async Task ExtendPackAsync(int packId)
        {
            var pack = await _pillIntakeCycleRepository.GetByIdAsync(packId);

            if (pack == null)
                throw new InvalidOperationException("Không tìm thấy pack.");

            if (pack.IsRenewed)
                throw new InvalidOperationException("Pack đã được gia hạn.");

            pack.IsRenewed = true;

            var newPack = new PillIntakeCycle
            {
                UserId = pack.UserId,
                StartDate = pack.StartDate.AddDays(pack.PackSize),
                PackSize = pack.PackSize
            };

            try
            {
                await _pillIntakeCycleRepository.AddAsync(newPack);
            }
            catch (InvalidOperationException ex)
            {
                pack.IsRenewed = false; // Rollback nếu cần
                throw new InvalidOperationException(ex.Message);
            }


        }
        public async Task<List<DateAndType>> LoadPillCalendar(int userId, int month, int year)
        {
            DateOnly targetDate;            
            if (IsValidDate(1, month, year))
            {
                targetDate = new DateOnly(year, month, 1);
            }
            else
            {
                throw new ArgumentException("Tháng năm này không hợp lệ.");
            }
            DateOnly startOfMonth = new DateOnly(year, month, 1);
            DateOnly endOfMonth = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
            DateOnly currentDate = startOfMonth;
            List<DateAndType> list = new List<DateAndType>();
            var packs = await _pillIntakeCycleRepository.GetAllPackRelatedToThisMonth(userId, month, year);
            while(currentDate <= endOfMonth)
            {
                string type = "ngày không có lịch";
                foreach (var pack in packs)
                {
                    if(currentDate >= pack.StartDate && currentDate <= pack.StartDate.AddDays(pack.PackSize - 1))
                    {
                        type = "ngày uống thuốc";
                    }
                }
                list.Add(new DateAndType
                {
                    Date = currentDate.ToString("dd"),
                    Type = type
                });
                currentDate = currentDate.AddDays(1);
            }
            return list;
        }

        private bool IsValidDate(int day, int month, int year)
        {
            try
            {
                var date = new DateOnly(year, month, day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task AddPackAsync(int userId, DateOnly startDate, int packSize)
        {           
            var newPack = new PillIntakeCycle
            {
                UserId = userId,
                StartDate = startDate,
                PackSize = packSize
            };

            try
            {
                await _pillIntakeCycleRepository.AddAsync(newPack);
            }
            catch (InvalidOperationException ex)
            {               
                throw new InvalidOperationException(ex.Message);
            }


        }
    }
}
