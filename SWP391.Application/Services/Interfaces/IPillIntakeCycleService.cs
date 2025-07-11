using SWP391.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Application.Services.Interfaces
{
    public interface IPillIntakeCycleService
    {
        Task ExtendPackAsync(int packId);
        Task<List<DateAndType>> LoadPillCalendar(int month, int year, int userId);

        Task AddPackAsync(int userId, DateOnly startDate, int packSize);


    }
}
