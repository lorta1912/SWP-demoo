using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SWP391.Application.Services.Interfaces;

namespace SWP391.Api.Services
{
    public class PillReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PillReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddHours(8);

                if (now > nextRun)
                {
                    nextRun = now.AddMinutes(1);
                }

                var delay = nextRun - now;
                if (delay.TotalMilliseconds < 0)
                {
                    delay = TimeSpan.Zero;
                }

                await Task.Delay(delay, stoppingToken);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var scopedReminder = scope.ServiceProvider.GetRequiredService<IContraceptivePillReminder>();
                    await scopedReminder.RunDailyPillReminderAsync();
                    await scopedReminder.SendRenewalRemindersAsync();
                }
            }
        }
    }
}