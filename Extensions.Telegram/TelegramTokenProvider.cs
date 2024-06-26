﻿using Boa.Identity.Telegram;

namespace Microsoft.AspNetCore.Identity;

/// <summary>
/// TokenProvider that generates tokens from the user's security stamp and notifies a user via telegram.
/// </summary>
/// <typeparam name="TUser">The type used to represent a user.</typeparam>
public class TelegramTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
    where TUser : class
{
    /// <summary>
    /// Checks if a two-factor authentication token can be generated for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> to retrieve the <paramref name="user"/> from.</param>
    /// <param name="user">The <typeparamref name="TUser"/> to check for the possibility of generating a two-factor authentication token.</param>
    /// <returns>True if the user has an email address set, otherwise false.</returns>
    public override async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        var telegramId = await manager.GetTelegramIdAsync(user).ConfigureAwait(false);
        return telegramId != null;
    }

    /// <summary>
    /// Returns the value for the user used as entropy in the generated token.
    /// </summary>
    /// <param name="purpose">The purpose of the two-factor authentication token.</param>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> to retrieve the <paramref name="user"/> from.</param>
    /// <param name="user">The <typeparamref name="TUser"/> to check for the possibility of generating a two-factor authentication token.</param>
    /// <returns>A string suitable for use as entropy in token generation.</returns>
    public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager,
        TUser user)
    {
        var telegramId = await manager.GetTelegramIdAsync(user).ConfigureAwait(false);
        return $"Telegram:{purpose}:{telegramId}";
    }
}
