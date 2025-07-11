using SWP391.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Application.Services.Interfaces
{
    public interface IMenstrualCycleService
    {
        public Task ToggleMenstrualDate(int UserId, int day, int month, int year);
        public Task<List<DateAndType>> LoadMenstrualCalendar(int UserId, int month, int year);

        public Task<string> GetNextMenstrualDate(int UserId);
        public Task UpdateMenstrualLengthAndCycleLength(int UserId, int MenstrualLength, int CycleLength);
    }
}
