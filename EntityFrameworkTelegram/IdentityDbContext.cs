using Boa.Identity.Telegram;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Boa.Identity.EntityFrameworkTelegram;

/// <summary>
/// Base class for the Entity Framework database context used for identity.
/// </summary>
/// <typeparam name="TUser">The type of the user objects.</typeparam>
public class IdentityDbContext<TUser> : IdentityDbContext<TUser, IdentityRole, string> where TUser : IdentityTelegramUser
{
    /// <summary>
    /// Initializes a new instance of <see cref="IdentityDbContext"/>.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public IdentityDbContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityDbContext" /> class.
    /// </summary>
    protected IdentityDbContext() { }
}
