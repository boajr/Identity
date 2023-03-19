using Boa.Identity.Telegram;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Boa.Identity.EntityFrameworkTelegram;

/// <summary>
/// Creates a new instance of a persistence store for the specified user type.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
public class TelegramUserOnlyStore<TUser> : TelegramUserOnlyStore<TUser, DbContext, string> where TUser : IdentityTelegramUser<string>, new()
{
    /// <summary>
    /// Constructs a new instance of <see cref="TelegramUserOnlyStore{TUser}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public TelegramUserOnlyStore(DbContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
public class TelegramUserOnlyStore<TUser, TContext> : TelegramUserOnlyStore<TUser, TContext, string>
    where TUser : IdentityTelegramUser<string>
    where TContext : DbContext
{
    /// <summary>
    /// Constructs a new instance of <see cref="UserStore{TUser, TRole, TContext}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public TelegramUserOnlyStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
public class TelegramUserOnlyStore<TUser, TContext, TKey> : TelegramUserOnlyStore<TUser, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>
    where TUser : IdentityTelegramUser<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Constructs a new instance of <see cref="UserStore{TUser, TRole, TContext, TKey}"/>.
    /// </summary>
    /// <param name="context">The <see cref="DbContext"/>.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
    public TelegramUserOnlyStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }
}

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TContext">The type of the data context class used to access the store.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
/// <typeparam name="TUserClaim">The type representing a claim.</typeparam>
/// <typeparam name="TUserLogin">The type representing a user external login.</typeparam>
/// <typeparam name="TUserToken">The type representing a user token.</typeparam>
public class TelegramUserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken> :
    UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>,
    IUserTelegramIdStore<TUser>
    where TUser : IdentityTelegramUser<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
{
    /// <summary>
    /// Creates a new instance of the store.
    /// </summary>
    /// <param name="context">The context used to access the store.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    public TelegramUserOnlyStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }

    /// <summary>
    /// Sets the telegram id for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telegramid should be set.</param>
    /// <param name="phoneNumber">The telegram id to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public virtual Task<long?> GetTelegramIdAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        return Task.FromResult(user.TelegramId);
    }

    /// <summary>
    /// Gets the telegram id, if any, for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telegram id should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telegram id, if any.</returns>
    public virtual Task SetTelegramIdAsync(TUser user, long? telegramId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public virtual Task<TUser?> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(Users.Where(u => u.PhoneNumber == phoneNumber).SingleOrDefault());
    }

    /// <summary>
    /// Gets the user, if any, associated with the specified telegram id.
    /// </summary>
    /// <param name="telegramId">The telegram id to return the user for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified telegram id.
    /// </returns>
    public virtual Task<TUser?> FindByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(Users.Where(u => u.TelegramId == telegramId).SingleOrDefault());
    }
}
