using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Application.Services.Interfaces
{
    public interface IContraceptivePillReminder
    {
        Task RunDailyPillReminderAsync();
        Task SendRenewalRemindersAsync();
        

    }
}
