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
public class TelegramUserOnlyStore<TUser, TContext, TKey> : TelegramUserOnlyStore<TUser, TContext, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityTelegramToken>
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
public class TelegramUserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken, TTelegramToken> :
    UserOnlyStore<TUser, TContext, TKey, TUserClaim, TUserLogin, TUserToken>,
    IUserTelegramIdStore<TUser>,
    ITelegramTokenStore
    where TUser : IdentityTelegramUser<TKey>
    where TContext : DbContext
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TTelegramToken : IdentityTelegramToken, new()
{
    /// <summary>
    /// Creates a new instance of the store.
    /// </summary>
    /// <param name="context">The context used to access the store.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    public TelegramUserOnlyStore(TContext context, IdentityErrorDescriber? describer = null) : base(context, describer) { }

    /// <summary>
    /// DbSet of telegram tokens.
    /// </summary>
    protected DbSet<TTelegramToken> TelegramTokens { get { return Context.Set<TTelegramToken>(); } }

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
        return user == null ? throw new ArgumentNullException(nameof(user)) : Task.FromResult(user.TelegramId);
    }

    /// <summary>
    /// Gets the telegram id, if any, for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telegram id should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telegram id, if any.</returns>
    public virtual Task SetTelegramIdAsync(TUser user, long? telegramId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.TelegramId = telegramId;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the user, if any, associated with the specified phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to return the user for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified phone number.
    /// </returns>
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



    /// <summary>
    /// Called to create a new instance of a <see cref="IdentityTelegramToken"/>.
    /// </summary>
    /// <param name="telegramId">The associated Telegram user.</param>
    /// <param name="loginProvider">The associated login provider.</param>
    /// <param name="name">The name of the user token.</param>
    /// <param name="value">The value of the user token.</param>
    /// <returns></returns>
    protected virtual TTelegramToken CreateTelegramToken(long telegramId, string loginProvider, string name, string? value)
    {
        return new TTelegramToken
        {
            TelegramId = telegramId,
            LoginProvider = loginProvider,
            Name = name,
            Value = value
        };
    }

    /// <summary>
    /// Find a telegram token if it exists.
    /// </summary>
    /// <param name="telegramId">The token owner.</param>
    /// <param name="loginProvider">The login provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The telegram token if it exists.</returns>
    protected virtual Task<TTelegramToken?> FindTelegramTokenAsync(long telegramId, string loginProvider, string name, CancellationToken cancellationToken)
        => TelegramTokens.FindAsync([telegramId, loginProvider, name], cancellationToken).AsTask();

    /// <summary>
    /// Add a new user token.
    /// </summary>
    /// <param name="token">The token to be added.</param>
    /// <returns></returns>
    protected virtual Task AddTelegramTokenAsync(TTelegramToken token)
    {
        TelegramTokens.Add(token);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Remove a new user token.
    /// </summary>
    /// <param name="token">The token to be removed.</param>
    /// <returns></returns>
    protected virtual Task RemoveTelegramTokenAsync(TTelegramToken token)
    {
        TelegramTokens.Remove(token);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the token value for a particular Telegram user.
    /// </summary>
    /// <param name="telegramId">The Telegram user id.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public virtual async Task SetTelegramTokenAsync(long telegramId, string loginProvider, string name, string? value, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var token = await FindTelegramTokenAsync(telegramId, loginProvider, name, cancellationToken).ConfigureAwait(false);
        if (token == null)
        {
            await AddTelegramTokenAsync(CreateTelegramToken(telegramId, loginProvider, name, value)).ConfigureAwait(false);
        }
        else
        {
            token.Value = value;
        }
    }

    /// <summary>
    /// Deletes a token for a Telegram user.
    /// </summary>
    /// <param name="telegramId">The Telegram user id.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public virtual async Task RemoveTelegramTokenAsync(long telegramId, string loginProvider, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var entry = await FindTelegramTokenAsync(telegramId, loginProvider, name, cancellationToken).ConfigureAwait(false);
        if (entry != null)
        {
            await RemoveTelegramTokenAsync(entry).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns the token value.
    /// </summary>
    /// <param name="telegramId">The Telegram user id.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public virtual async Task<string?> GetTelegramTokenAsync(long telegramId, string loginProvider, string name, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var entry = await FindTelegramTokenAsync(telegramId, loginProvider, name, cancellationToken).ConfigureAwait(false);
        return entry?.Value;
    }

    /// <summary>
    /// Returns all tokens.
    /// </summary>
    /// <param name="telegramId">The Telegram user id.</param>
    /// <param name="loginProvider">The authentication provider for tokens.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public virtual async Task<(string Name, string? Value)[]> GetAllTelegramTokensAsync(long telegramId, string loginProvider, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return (await TelegramTokens
            .Where(tt => tt.TelegramId == telegramId && tt.LoginProvider == loginProvider)
            .Select(tt => new { tt.Name, tt.Value }).ToListAsync(cancellationToken)).Select(tt => (tt.Name, tt.Value)).ToArray();
    }

    /// <summary>
    /// Confirm the modifications in the user store.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the save operation.</returns>
    public virtual async Task<IdentityResult> SaveChangesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        try
        {
            await SaveChanges(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }
        return IdentityResult.Success;
    }
}
