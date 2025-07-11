using Microsoft.Extensions.Configuration;
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
    public class MenstrualCycleService : IMenstrualCycleService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMenstrualCycleRepository _menstrualCycleRepository;       
        private readonly AppDbContext _context; // Thêm AppDbContext
        

        public MenstrualCycleService(
            IUserRepository userRepository,
            IMenstrualCycleRepository menstrualCycleRepository,
            AppDbContext context)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _menstrualCycleRepository = menstrualCycleRepository ?? throw new ArgumentNullException(nameof(menstrualCycleRepository));           
            _context = context ?? throw new ArgumentNullException(nameof(context));
            
        }
        public async Task ToggleMenstrualDate(int UserId, int day, int month, int year)
        {
            DateOnly targetDate;
            if (IsValidDate(day, month, year))
            {
                targetDate = new DateOnly(year, month, day);
            }
            else
            {
                throw new ArgumentException("Ngày này không hợp lệ.");
            }
            // Tìm các chu kỳ liên quan
            var cycleThis = await _menstrualCycleRepository.GetMenstrualCycleByUserIdThatIncludeThatDay(UserId, targetDate);
            var cyclePrev = await _menstrualCycleRepository.GetMenstrualCycleByUserIdThatThatDayIsAfterEndDate(UserId, targetDate);
            var cycleNext = await _menstrualCycleRepository.GetMenstrualCycleByUserIdThatThatDayIsBeforeStartDate(UserId, targetDate);

            // Xử lý logic toggle
            if (cycleThis != null)
            {
                // Nếu ngày đang nằm trong chu kỳ
                if (cycleThis.StartDate == cycleThis.EndDate)
                {
                    await _menstrualCycleRepository.DeleteMenstrualCycleAsync(cycleThis);
                }
                else if (targetDate == cycleThis.StartDate)
                {
                    cycleThis.StartDate = cycleThis.StartDate.AddDays(1);
                    await _menstrualCycleRepository.UpdateMenstrualCycleAsync(cycleThis);
                }
                else if (targetDate == cycleThis.EndDate)
                {
                    cycleThis.EndDate = cycleThis.EndDate.AddDays(-1);
                    await _menstrualCycleRepository.UpdateMenstrualCycleAsync(cycleThis);
                }
                else
                {
                    // Nằm giữa chu kỳ -> tách làm 2
                    var newCycle = new MenstrualCycle
                    {
                        UserId = UserId,
                        StartDate = targetDate.AddDays(1),
                        EndDate = cycleThis.EndDate
                    };

                    cycleThis.EndDate = targetDate.AddDays(-1);
                    await _menstrualCycleRepository.UpdateMenstrualCycleAsync(cycleThis);
                    await _menstrualCycleRepository.AddMenstrualCycleAsync(newCycle);
                }
            }
            else
            {
                // Nếu ngày không thuộc chu kỳ nào
                if (cyclePrev != null && cycleNext != null)
                {
                    cyclePrev.EndDate = cycleNext.EndDate;                   
                    await _menstrualCycleRepository.DeleteMenstrualCycleAsync(cycleNext);
                }
                else if (cyclePrev != null)
                {
                    cyclePrev.EndDate = targetDate;
                    await _menstrualCycleRepository.UpdateMenstrualCycleAsync(cyclePrev);
                }
                else if (cycleNext != null)
                {
                    cycleNext.StartDate = targetDate;
                    await _menstrualCycleRepository.UpdateMenstrualCycleAsync(cycleNext);
                }
                else
                {
                    await _menstrualCycleRepository.AddMenstrualCycleAsync(new MenstrualCycle
                    {
                        UserId = UserId,
                        StartDate = targetDate,
                        EndDate = targetDate
                    });
                }
            }                       
        }

        public async Task<List<DateAndType>> LoadMenstrualCalendar(int UserId, int month, int year)
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
            

            var user = await _userRepository.GetUserByIdAsync(UserId);
            if (user.CycleLength == null || user.MenstrualLength == null)
            {
                throw new ArgumentException("Thiếu thông tin CycleLength hoặc MenstrualLength.");
            }
            int nextMonth, nextYear;
            if (month == 12)
            {
                nextMonth = 1;
                nextYear = year + 1;
            }
            else
            {
                nextMonth = month + 1;
                nextYear = year;
            }

            int cycleLength = user.CycleLength.Value;
            int menstrualLength = user.MenstrualLength.Value;

            var lastCycle = await _menstrualCycleRepository.TakeLastCycleByUserIdAsync(UserId);

            if (lastCycle == null)
            {
                throw new ArgumentException("Không có dữ liệu về chu kì gần nhất.");
            }

            

            var today = DateOnly.FromDateTime(DateTime.Today);
            DateOnly startOfMonth = new DateOnly(year, month, 1);
            DateOnly endOfMonth = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
            DateOnly currentDate = startOfMonth;
            DateOnly lastStartDate = lastCycle.StartDate;
            DateOnly lastEndDate = lastCycle.EndDate;

            int diffDays = (lastEndDate.ToDateTime(TimeOnly.MinValue) - lastStartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
            bool checkIfCycleFilled = diffDays <= menstrualLength;
            bool checkIfTodayNearLastCycleEndDate = today == lastEndDate || today.AddDays(-1) == lastEndDate;
            var list = new List<DateAndType>();

            var PredictCycle = new PredictCycle();
            PredictCycle.ovulationDay = lastCycle.StartDate.AddDays(cycleLength - 14);
            PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
            PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
            PredictCycle.nextPeriod = lastCycle.StartDate.AddDays(cycleLength);
            PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
            //load lịch thực tế
            if (year < DateTime.Now.Year || (year == DateTime.Now.Year && month < DateTime.Now.Month))
            {
                string type = "ngày trắng";
                var cycles1 = await _menstrualCycleRepository.TakeCyclesByMonthAsync(UserId, month,year);
                if(cycles1 != null)
                {
                    while (currentDate <= endOfMonth)
                    {

                        foreach (var cycle in cycles1)
                        {
                            if (cycle.StartDate <= currentDate && currentDate <= cycle.EndDate)
                            {
                                type = "ngày hành kinh thực tế";
                                break;
                            }
                        }
                        list.Add(new DateAndType
                        {
                            Date = currentDate.ToString("dd"),
                            Type = type
                        });
                        currentDate = currentDate.AddDays(1);
                    }
                }
                else
                {
                    while (currentDate <= endOfMonth)
                    {
                        list.Add(new DateAndType
                        {
                            Date = currentDate.ToString("dd"),
                            Type = type
                        });
                        currentDate = currentDate.AddDays(1);
                    }
                }
                    

            }
            //load lịch thực tế và dự đoán
            else if (month == DateTime.Now.Month && year == DateTime.Now.Year)
            {
                var cycles1 = await _menstrualCycleRepository.TakeCyclesByMonthAsync(UserId, month, year);               
                if (cycles1 != null)
                {

                    while (currentDate <= lastCycle.EndDate)
                    {
                        string type = "ngày trắng";
                        foreach (var cycle in cycles1)
                        {
                            if (cycle.StartDate <= currentDate && currentDate <= cycle.EndDate)
                            {
                                type = "ngày hành kinh thực tế";

                            }
                        }
                        list.Add(new DateAndType
                        {
                            Date = currentDate.ToString("dd"),
                            Type = type
                        });
                        currentDate = currentDate.AddDays(1);
                    }

                    if (checkIfCycleFilled && checkIfTodayNearLastCycleEndDate)
                    {
                        while (currentDate <= endOfMonth)
                        {
                            if (currentDate <= lastStartDate.AddDays(menstrualLength - 1))
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày hành kinh dự đoán"
                                });


                            }
                            if (currentDate > lastStartDate.AddDays(menstrualLength - 1) && currentDate < PredictCycle.fertileStart)
                            {

                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tương đốiii"
                                });


                            }
                            if (currentDate >= PredictCycle.fertileStart && currentDate <= PredictCycle.fertileEnd)
                            {
                                if (currentDate == PredictCycle.ovulationDay)
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày rụng trứng"
                                    });


                                }
                                else
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày dễ thụ thai"
                                    });


                                }
                            }
                            if (currentDate > PredictCycle.fertileEnd && currentDate < PredictCycle.nextPeriod)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tuyệt đối"
                                });


                            }
                            if (currentDate >= PredictCycle.nextPeriod && currentDate <= PredictCycle.endNextPeriod)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày hành kinh dự đoán"
                                });

                            }
                            if (currentDate == PredictCycle.endNextPeriod)
                            {
                                PredictCycle.ovulationDay = PredictCycle.nextPeriod.AddDays(cycleLength - 14);
                                PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
                                PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
                                PredictCycle.nextPeriod = PredictCycle.nextPeriod.AddDays(cycleLength);
                                PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
                            }

                            currentDate = currentDate.AddDays(1);

                        }
                    }
                    else
                    {
                        while (currentDate <= endOfMonth)
                        {
                            if (currentDate < PredictCycle.fertileStart)
                            {

                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tương đối"
                                });


                            }
                            if (currentDate >= PredictCycle.fertileStart && currentDate <= PredictCycle.fertileEnd)
                            {
                                if (currentDate == PredictCycle.ovulationDay)
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày rụng trứng"
                                    });


                                }
                                else
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày dễ thụ thai"
                                    });


                                }
                            }
                            if (currentDate > PredictCycle.fertileEnd && currentDate < PredictCycle.nextPeriod)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tuyệt đối"
                                });


                            }
                            if (currentDate >= PredictCycle.nextPeriod && currentDate <= PredictCycle.endNextPeriod)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày hành kinh dự đoán"
                                });

                            }
                            if (currentDate == PredictCycle.endNextPeriod)
                            {
                                PredictCycle.ovulationDay = PredictCycle.nextPeriod.AddDays(cycleLength - 14);
                                PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
                                PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
                                PredictCycle.nextPeriod = PredictCycle.nextPeriod.AddDays(cycleLength);
                                PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
                            }

                            currentDate = currentDate.AddDays(1);


                        }
                    }
                }
                else
                {
                    if (checkIfCycleFilled && checkIfTodayNearLastCycleEndDate)
                    {
                        while (currentDate <= endOfMonth)
                        {
                            if (currentDate <= lastStartDate.AddDays(menstrualLength - 1))
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày hành kinh dự đoán"
                                });


                            }
                            if (currentDate > lastStartDate.AddDays(menstrualLength - 1) && currentDate < PredictCycle.fertileStart)
                            {

                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tương đốiii"
                                });


                            }
                            if (currentDate >= PredictCycle.fertileStart && currentDate <= PredictCycle.fertileEnd)
                            {
                                if (currentDate == PredictCycle.ovulationDay)
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày rụng trứng"
                                    });


                                }
                                else
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày dễ thụ thai"
                                    });


                                }
                            }
                            if (currentDate > PredictCycle.fertileEnd && currentDate < PredictCycle.nextPeriod)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tuyệt đối"
                                });


                            }
                            if (currentDate >= PredictCycle.nextPeriod && currentDate <= PredictCycle.endNextPeriod)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày hành kinh dự đoán"
                                });

                            }
                            if (currentDate == PredictCycle.endNextPeriod)
                            {
                                PredictCycle.ovulationDay = PredictCycle.nextPeriod.AddDays(cycleLength - 14);
                                PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
                                PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
                                PredictCycle.nextPeriod = PredictCycle.nextPeriod.AddDays(cycleLength);
                                PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
                            }

                            currentDate = currentDate.AddDays(1);

                        }
                    }
                    else
                    {
                        while (currentDate <= endOfMonth)
                        {
                            if (currentDate < PredictCycle.fertileStart)
                            {

                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tương đối"
                                });


                            }
                            if (currentDate >= PredictCycle.fertileStart && currentDate <= PredictCycle.fertileEnd)
                            {
                                if (currentDate == PredictCycle.ovulationDay)
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày rụng trứng"
                                    });


                                }
                                else
                                {
                                    list.Add(new DateAndType
                                    {
                                        Date = currentDate.ToString("dd"),
                                        Type = "ngày dễ thụ thai"
                                    });


                                }
                            }
                            if (currentDate > PredictCycle.fertileEnd && currentDate < PredictCycle.nextPeriod.AddDays(cycleLength))
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày an toàn tuyệt đối"
                                });


                            }
                            if (currentDate >= PredictCycle.nextPeriod && currentDate <= PredictCycle.endNextPeriod)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày hành kinh dự đoán"
                                });

                            }
                            if (currentDate == PredictCycle.endNextPeriod)
                            {
                                PredictCycle.ovulationDay = PredictCycle.nextPeriod.AddDays(cycleLength - 14);
                                PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
                                PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
                                PredictCycle.nextPeriod = PredictCycle.nextPeriod.AddDays(cycleLength);
                                PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
                            }

                            currentDate = currentDate.AddDays(1);


                        }
                    }
                }
                //load lịch dự đoán    
            }
            else
            {
                while (
                    (PredictCycle.nextPeriod.AddDays(menstrualLength - 1).Month != month || PredictCycle.nextPeriod.AddDays(menstrualLength - 1).Year != year)
                    &&
                    (PredictCycle.nextPeriod.AddDays(menstrualLength - 1).Month != nextMonth || PredictCycle.nextPeriod.AddDays(menstrualLength - 1).Year != nextYear)
                    )
                {
                    PredictCycle.ovulationDay = PredictCycle.nextPeriod.AddDays(cycleLength - 14);
                    PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
                    PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
                    PredictCycle.nextPeriod = PredictCycle.nextPeriod.AddDays(cycleLength);
                    PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
                }


                if (checkIfCycleFilled && checkIfTodayNearLastCycleEndDate)
                {
                    while (currentDate <= endOfMonth)
                    {
                        if (currentDate <= lastStartDate.AddDays(menstrualLength - 1))
                        {
                            list.Add(new DateAndType
                            {
                                Date = currentDate.ToString("dd"),
                                Type = "ngày hành kinh dự đoán"
                            });


                        }
                        if (currentDate > lastStartDate.AddDays(menstrualLength - 1) && currentDate < PredictCycle.fertileStart)
                        {

                            list.Add(new DateAndType
                            {
                                Date = currentDate.ToString("dd"),
                                Type = "ngày an toàn tương đốiii"
                            });
                        }
                        if (currentDate >= PredictCycle.fertileStart && currentDate <= PredictCycle.fertileEnd)
                        {
                            if (currentDate == PredictCycle.ovulationDay)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày rụng trứng"
                                });


                            }
                            else
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày dễ thụ thai"
                                });


                            }
                        }
                        if (currentDate > PredictCycle.fertileEnd && currentDate < PredictCycle.nextPeriod)
                        {
                            list.Add(new DateAndType
                            {
                                Date = currentDate.ToString("dd"),
                                Type = "ngày an toàn tuyệt đối"
                            });


                        }
                        if (currentDate >= PredictCycle.nextPeriod && currentDate <= PredictCycle.endNextPeriod)
                        {
                            list.Add(new DateAndType
                            {
                                Date = currentDate.ToString("dd"),
                                Type = "ngày hành kinh dự đoán"
                            });

                        }
                        if (currentDate == PredictCycle.endNextPeriod)
                        {
                            PredictCycle.ovulationDay = PredictCycle.nextPeriod.AddDays(cycleLength - 14);
                            PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
                            PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
                            PredictCycle.nextPeriod = PredictCycle.nextPeriod.AddDays(cycleLength);
                            PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
                        }

                        currentDate = currentDate.AddDays(1);

                    }
                }
                else
                {
                    while (currentDate <= endOfMonth)
                    {
                        if (currentDate < PredictCycle.fertileStart)
                        {

                            list.Add(new DateAndType
                            {
                                Date = currentDate.ToString("dd"),
                                Type = "ngày an toàn tương đối"
                            });


                        }
                        if (currentDate >= PredictCycle.fertileStart && currentDate <= PredictCycle.fertileEnd)
                        {
                            if (currentDate == PredictCycle.ovulationDay)
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày rụng trứng"
                                });


                            }
                            else
                            {
                                list.Add(new DateAndType
                                {
                                    Date = currentDate.ToString("dd"),
                                    Type = "ngày dễ thụ thai"
                                });


                            }
                        }
                        if (currentDate > PredictCycle.fertileEnd && currentDate < PredictCycle.nextPeriod)
                        {
                            list.Add(new DateAndType
                            {
                                Date = currentDate.ToString("dd"),
                                Type = "ngày an toàn tuyệt đối"
                            });


                        }
                        if (currentDate >= PredictCycle.nextPeriod && currentDate <= PredictCycle.endNextPeriod)
                        {
                            list.Add(new DateAndType
                            {
                                Date = currentDate.ToString("dd"),
                                Type = "ngày hành kinh dự đoán"
                            });

                        }
                        if (currentDate == PredictCycle.endNextPeriod)
                        {
                            PredictCycle.ovulationDay = PredictCycle.nextPeriod.AddDays(cycleLength - 14);
                            PredictCycle.fertileStart = PredictCycle.ovulationDay.AddDays(-5);
                            PredictCycle.fertileEnd = PredictCycle.ovulationDay.AddDays(1);
                            PredictCycle.nextPeriod = PredictCycle.nextPeriod.AddDays(cycleLength);
                            PredictCycle.endNextPeriod = PredictCycle.nextPeriod.AddDays(menstrualLength - 1);
                        }

                        currentDate = currentDate.AddDays(1);


                    }
                }
            }
            
            return (list);
        }

        public async Task<string> GetNextMenstrualDate(int UserId)
        {
            var user = await _userRepository.GetUserByIdAsync(UserId);
            if (user.CycleLength == null || user.MenstrualLength == null)
            {
                throw new ArgumentException("Thiếu thông tin CycleLength hoặc MenstrualLength.");
            }
            var lastCycle = await _menstrualCycleRepository.TakeLastCycleByUserIdAsync(UserId);
            if (lastCycle == null)
            {
                throw new ArgumentException("Không có dữ liệu về chu kì gần nhất.");
            }
            return lastCycle.StartDate.AddDays(user.CycleLength.Value).ToString("dd-MM-yyyy");
        }

        public async Task UpdateMenstrualLengthAndCycleLength(int UserId, int MenstrualLength, int CycleLength)
        {
            var user = await _userRepository.GetUserByIdAsync(UserId);
            user.MenstrualLength = MenstrualLength;
            user.CycleLength = CycleLength;
            await _userRepository.UpdateUserAsync(user);
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
    }
}
