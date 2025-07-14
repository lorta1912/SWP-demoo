using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWP391.Application.Services.Interfaces;
using Quartz;

namespace SWP391.Application.Jobs
{
    public class PillReminderJob : IJob
    {
        private readonly IContraceptivePillReminder _pillReminder;

        public PillReminderJob(IContraceptivePillReminder pillReminder)
        {
            _pillReminder = pillReminder;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _pillReminder.RunDailyPillReminderAsync();
            await _pillReminder.SendRenewalRemindersAsync();
        }
    }
}
