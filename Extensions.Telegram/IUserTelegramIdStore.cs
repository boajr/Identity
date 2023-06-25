using Microsoft.AspNetCore.Identity;

namespace Boa.Identity.Telegram;

/// <summary>
/// Provides an abstraction for a store containing users' telegram ids.
/// </summary>
/// <typeparam name="TUser">The type encapsulating a user.</typeparam>
public interface IUserTelegramIdStore<TUser> : IUserStore<TUser> where TUser : class
{
    /// <summary>
    /// Sets the telegram id for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telegram id should be set.</param>
    /// <param name="phoneNumber">The telegram id to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetTelegramIdAsync(TUser user, long? telegramId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the telegram id, if any, for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telegram id should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telegram id, if any.</returns>
    Task<long?> GetTelegramIdAsync(TUser user, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the user, if any, associated with the specified phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to return the user for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified phone number.
    /// </returns>
    Task<TUser?> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the user, if any, associated with the specified telegram id.
    /// </summary>
    /// <param name="telegramId">The telegram id to return the user for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified telegram id.
    /// </returns>
    Task<TUser?> FindByTelegramIdAsync(long telegramId, CancellationToken cancellationToken);
}
