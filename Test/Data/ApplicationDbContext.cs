﻿using Boa.Identity.EntityFrameworkTelegram;
using Boa.Identity.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Test.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityTelegramUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}