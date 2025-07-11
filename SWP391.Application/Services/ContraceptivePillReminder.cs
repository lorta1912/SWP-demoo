using Microsoft.Extensions.Configuration;
using SWP391.Application.Services.Interfaces;
using SWP391.Infrastructure.Data;
using SWP391.Infrastructure.Entities;
using SWP391.Infrastructure.Repositories;
using SWP391.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Application.Services
{
    public class ContraceptivePillReminder : IContraceptivePillReminder
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPillIntakeCycleRepository _pillIntakeCycleRepository;

        public ContraceptivePillReminder(AppDbContext context, IEmailService emailService, IPillIntakeCycleRepository pillIntakeCycleRepository)
        {
            
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _pillIntakeCycleRepository = pillIntakeCycleRepository ?? throw new ArgumentNullException(nameof(pillIntakeCycleRepository));
        }

        public async Task RunDailyPillReminderAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var packs = await _pillIntakeCycleRepository.GetOngoingPillIntakeCycleAsync(today);

            foreach (var pack in packs)
            {
                var user = pack.User;
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var daysTaken = (today.DayNumber - pack.StartDate.DayNumber);
                    var subject = "Nhắc nhở uống thuốc tránh thai";
                    var body = $@"
                                Xin chào {user.FullName ?? "bạn"},<br/>
                                Hôm nay là ngày {today:dd/MM/yyyy} bạn nhớ uống thuốc tránh thai nhé!<br/>
                                Bạn đã uống được {daysTaken} viên kể từ ngày {pack.StartDate:dd/MM/yyyy} rồi đấy ❤️";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
            }

            Console.WriteLine($"[ContraceptivePillReminder] Đã gửi {packs.Count} mail nhắc.");
        }

        public async Task SendRenewalRemindersAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var expiringPacks = await _pillIntakeCycleRepository.GetExpiringPillIntakeCycleAsync(today);

            foreach (var pack in expiringPacks)
            {
                var extendLink = $"https://localhost:7100/api/PillIntakeCycle/extend-pill-pack?packId={pack.Id}";

                var body = $@"
Xin chào {pack.User.FullName},<br/>
Bạn còn 3 ngày nữa là hết liều thuốc tránh thai.<br/>
Bạn muốn tạo gói mới sau khi hết gói này không?<br/><br/>
<a href='{extendLink}'
   style='display:inline-block;padding:12px 20px;background-color:#28a745;color:white;
   text-decoration:none;border-radius:5px;'>Tôi muốn</a>";

                await _emailService.SendEmailAsync(
                    pack.User.Email,
                    "Sắp hết liều thuốc tránh thai",
                    body
                );
            }
        }

        
    }
}
