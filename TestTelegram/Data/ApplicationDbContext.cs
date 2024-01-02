using Boa.Identity.EntityFrameworkTelegram;
using Boa.Identity.Telegram;
using Microsoft.EntityFrameworkCore;

namespace TestTelegram.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityTelegramUser, IdentityTelegramToken>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
