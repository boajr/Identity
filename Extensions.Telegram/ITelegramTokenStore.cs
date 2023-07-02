using Microsoft.AspNetCore.Identity;

namespace Boa.Identity.Telegram;

/// <summary>
/// Provides an abstraction to store TelegramId tokens.
/// </summary>
public interface ITelegramTokenStore
    {
    /// <summary>
    /// Sets the token value for a particular Telegram user.
    /// </summary>
    /// <param name="telegramId">The Telegram user id.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SetTelegramTokenAsync(long telegramId, string loginProvider, string name, string? value, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a token for a Telegram user.
    /// </summary>
    /// <param name="telegramId">The Telegram user id.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task RemoveTelegramTokenAsync(long telegramId, string loginProvider, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the token value.
    /// </summary>
    /// <param name="telegramId">The Telegram user id.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task<string?> GetTelegramTokenAsync(long telegramId, string loginProvider, string name, CancellationToken cancellationToken);

    /// <summary>
    /// Confirm the modifications in the user store.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the save operation.</returns>
    Task<IdentityResult> SaveChangesAsync(CancellationToken cancellationToken);
}
