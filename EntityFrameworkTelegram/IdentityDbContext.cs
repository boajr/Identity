using Boa.Identity.Telegram;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Boa.Identity.EntityFrameworkTelegram;

/// <summary>
/// Base class for the Entity Framework database context used for identity.
/// </summary>
/// <typeparam name="TUser">The type of the user objects.</typeparam>
public class IdentityDbContext<TUser, TTelegramToken> : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<TUser, IdentityRole, string>
    where TUser : IdentityTelegramUser
    where TTelegramToken : IdentityTelegramToken
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

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TTelegramToken}"/> of Telegram tokens.
    /// </summary>
    public virtual DbSet<TTelegramToken> TelegramToken { get; set; } = default!;

    private StoreOptions? GetStoreOptions() => this.GetService<IDbContextOptions>()
                        .Extensions.OfType<CoreOptionsExtension>()
                        .FirstOrDefault()?.ApplicationServiceProvider
                        ?.GetService<IOptions<IdentityOptions>>()
                        ?.Value?.Stores;

    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// </summary>
    /// <param name="builder">
    /// The builder being used to construct the model for this context.
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var storeOptions = GetStoreOptions();
        var maxKeyLength = storeOptions?.MaxLengthForKeys ?? 0;

        builder.Entity<TTelegramToken>(b =>
        {
            b.HasKey(t => new { t.TelegramId, t.LoginProvider, t.Name });

            if (maxKeyLength > 0)
            {
                b.Property(t => t.LoginProvider).HasMaxLength(maxKeyLength);
                b.Property(t => t.Name).HasMaxLength(maxKeyLength);
            }

            //if (encryptPersonalData)
            //{
            //    var tokenProps = typeof(TUserToken).GetProperties().Where(
            //                    prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
            //    foreach (var p in tokenProps)
            //    {
            //        if (p.PropertyType != typeof(string))
            //        {
            //            throw new InvalidOperationException(Resources.CanOnlyProtectStrings);
            //        }
            //        b.Property(typeof(string), p.Name).HasConversion(converter);
            //    }
            //}

            b.ToTable("AspNetTelegramTokens");
        });
    }
}
