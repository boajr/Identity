using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Boa.Identity.Telegram
{
    public static class UserManagerExtensions
    {
        /// <summary>
        /// Gets a flag indicating whether the backing user store supports user telegram ids.
        /// </summary>
        /// <value>
        /// true if the backing user store supports user telegram ids, otherwise false.
        /// </value>
        public static bool SupportsUserTelegramId<TUser>(this UserManager<TUser> userManager) where TUser : class
        {
            ThrowIfDisposed(userManager);
            return userManager
                   .GetType()
                   .GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance)?
                   .GetMethod?
                   .Invoke(userManager, null) is IUserTelegramIdStore<TUser>;
        }

        /// <summary>
        /// Gets a flag indicating whether the backing user store supports Telegram tokens.
        /// </summary>
        /// <value>
        /// true if the backing user store supports Telegram tokens, otherwise false.
        /// </value>
        public static bool SupportsTelegramTokens<TUser>(this UserManager<TUser> userManager) where TUser : class
        {
            ThrowIfDisposed(userManager);
            return userManager
                   .GetType()
                   .GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance)?
                   .GetMethod?
                   .Invoke(userManager, null) is ITelegramTokenStore;
        }

        /// <summary>
        /// Gets the user, if any, associated with the specified phone number.
        /// Note: Its recommended that identityOptions.User.RequireUniquePhoneNumber be set to true when using this method, otherwise
        /// the store may throw if there are users with duplicate phone numbers.
        /// </summary>
        /// <param name="phoneNumber">The phone number to return the user for.</param>
        /// <returns>
        /// The task object containing the results of the asynchronous lookup operation, the user, if any, associated with the specified phone number.
        /// </returns>
        public static async Task<TUser?> FindByPhoneNumberAsync<TUser>(this UserManager<TUser> userManager, string phoneNumber) where TUser : class
        {
            ThrowIfDisposed(userManager);
            ArgumentNullException.ThrowIfNull(phoneNumber);

            var store = GetTelegramIdStore(userManager);
            var cancellationToken = GetCancellationToken(userManager);
            var user = await store.FindByPhoneNumberAsync(phoneNumber, cancellationToken).ConfigureAwait(false);

            // Need to potentially check all keys
            if (user == null && userManager.Options.Stores.ProtectPersonalData)
            {
                var services = GetServiceProvider(userManager);
                var keyRing = services.GetService<ILookupProtectorKeyRing>();
                var protector = services.GetService<ILookupProtector>();
                if (keyRing != null && protector != null)
                {
                    foreach (var key in keyRing.GetAllKeyIds())
                    {
                        var oldKey = protector.Protect(key, phoneNumber);
                        user = await store.FindByPhoneNumberAsync(oldKey, cancellationToken).ConfigureAwait(false);
                        if (user != null)
                        {
                            return user;
                        }
                    }
                }
            }
            return user;
        }

        /// <summary>
        /// Gets the user, if any, associated with the specified telegram id.
        /// Note: Its recommended that identityOptions.User.RequireUniqueTelegramId be set to true when using this method, otherwise
        /// the store may throw if there are users with duplicate telegram ids.
        /// </summary>
        /// <param name="email">The email address to return the user for.</param>
        /// <returns>
        /// The task object containing the results of the asynchronous lookup operation, the user, if any, associated with the specified telegram id.
        /// </returns>
        public static async Task<TUser?> FindByTelegramIdAsync<TUser>(this UserManager<TUser> userManager, long telegramId, bool askToRegister = false, ITelegramBotClient? botClient = null) where TUser : class
        {
            ThrowIfDisposed(userManager);
            var store = GetTelegramIdStore(userManager);

            var cancellationToken = GetCancellationToken(userManager);
            var services = GetServiceProvider(userManager);
            var user = await store.FindByTelegramIdAsync(telegramId, cancellationToken).ConfigureAwait(false);

            //capire se è possibile proteggere gli id!!!
            //capire se posso aggiungere le opzioni identityOptions.User.RequireUniqueTelegramId e identityOptions.User.RequireUniquePhoneNumber

            // Need to potentially check all keys
            //if (user == null && userManager.Options.Stores.ProtectPersonalData)
            //{
            //    var services = GetServiceProvider(userManager);
            //    var keyRing = services.GetService<ILookupProtectorKeyRing>();
            //    var protector = services.GetService<ILookupProtector>();
            //    if (keyRing != null && protector != null)
            //    {
            //        foreach (var key in keyRing.GetAllKeyIds())
            //        {
            //            var oldKey = protector.Protect(key, telegramId);
            //            user = await store.FindByTelegramIdAsync(oldKey, cancellationToken).ConfigureAwait(false);
            //            if (user != null)
            //            {
            //                return user;
            //            }
            //        }
            //    }
            //}

            if (user == null && askToRegister && botClient != null)
            {
                IStringLocalizer localizer = new IdentityStringLocalizer(services, "Boa.Identity.ResetPasswordService");

                var msg = await botClient.SendTextMessageAsync(
                    chatId: telegramId,
                    text: localizer["Unknown user!\r\nPlease post your contact card to be identified"],
                    replyMarkup: new ReplyKeyboardMarkup(new[] {
                        new KeyboardButton[] { KeyboardButton.WithRequestContact(localizer["Send\r\nCONTACT CARD"]) },
                        new KeyboardButton[] { new KeyboardButton(localizer["Cancel"]) }
                    })
                ).ConfigureAwait(false);

                // save messageid to check response from user
                await userManager.SetTelegramTokenAsync(telegramId, "[boaidentity]", "RegisterUserMessage", msg.MessageId.ToString()).ConfigureAwait(false);
            }

            return user;
        }

        /// <summary>
        /// Gets the telegram id, if any, for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose telegram id should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telegram id, if any.</returns>
        public static async Task<long?> GetTelegramIdAsync<TUser>(this UserManager<TUser> userManager, TUser user) where TUser : class
        {
            ThrowIfDisposed(userManager);
            ArgumentNullException.ThrowIfNull(user);

            var store = GetTelegramIdStore(userManager);
            return await store.GetTelegramIdAsync(user, GetCancellationToken(userManager)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the telegram id for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose telegram id to set.</param>
        /// <param name="telegramId">The telegram id to set.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>
        /// of the operation.
        /// </returns>
        public static async Task<IdentityResult> SetTelegramIdAsync<TUser>(this UserManager<TUser> userManager, TUser user, long? telegramId) where TUser : class
        {
            ThrowIfDisposed(userManager);
            ArgumentNullException.ThrowIfNull(user);

            var store = GetTelegramIdStore(userManager);
            await store.SetTelegramIdAsync(user, telegramId, GetCancellationToken(userManager)).ConfigureAwait(false);
            return await userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a Telegram token for a user.
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <returns>The authentication token for a user</returns>
        public static Task<string?> GetTelegramTokenAsync<TUser>(this UserManager<TUser> userManager, long telegramId, string loginProvider, string tokenName) where TUser : class
        {
            ThrowIfDisposed(userManager);
            ArgumentNullException.ThrowIfNull(loginProvider);
            ArgumentNullException.ThrowIfNull(tokenName);

            var store = GetTelegramTokenStore(userManager);
            return store.GetTelegramTokenAsync(telegramId, loginProvider, tokenName, GetCancellationToken(userManager));
        }

        /// <summary>
        /// Returns all Telegram tokens for a user.
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <returns>The authentication token for a user</returns>
        public static Task<(string Name, string? Value)[]> GetAllTelegramTokensAsync<TUser>(this UserManager<TUser> userManager, long telegramId, string loginProvider) where TUser : class
        {
            ThrowIfDisposed(userManager);
            ArgumentNullException.ThrowIfNull(loginProvider);

            var store = GetTelegramTokenStore(userManager);
            return store.GetAllTelegramTokensAsync(telegramId, loginProvider, GetCancellationToken(userManager));
        }

        /// <summary>
        /// Sets a Telegram token for a user.
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <param name="tokenValue">The value of the token.</param>
        /// <returns>Whether the user was successfully updated.</returns>
        public static async Task<IdentityResult> SetTelegramTokenAsync<TUser>(this UserManager<TUser> userManager, long telegramId, string loginProvider, string tokenName, string? tokenValue) where TUser : class
        {
            ThrowIfDisposed(userManager);
            ArgumentNullException.ThrowIfNull(loginProvider);
            ArgumentNullException.ThrowIfNull(tokenName);

            // REVIEW: should updating any tokens affect the security stamp?
            var store = GetTelegramTokenStore(userManager);
            var cancellationToken = GetCancellationToken(userManager);
            await store.SetTelegramTokenAsync(telegramId, loginProvider, tokenName, tokenValue, cancellationToken).ConfigureAwait(false);
            return await store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove a Telegram token for a user.
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <returns>Whether a token was removed.</returns>
        public static async Task<IdentityResult> RemoveTelegramTokenAsync<TUser>(this UserManager<TUser> userManager, long telegramId, string loginProvider, string tokenName) where TUser : class
        {
            ThrowIfDisposed(userManager);
            ArgumentNullException.ThrowIfNull(loginProvider);
            ArgumentNullException.ThrowIfNull(tokenName);

            var store = GetTelegramTokenStore(userManager);
            var cancellationToken = GetCancellationToken(userManager);
            await store.RemoveTelegramTokenAsync(telegramId, loginProvider, tokenName, cancellationToken).ConfigureAwait(false);
            return await store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static void ThrowIfDisposed<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            MethodInfo throwIfDisposed = userManager.GetType().GetMethod("ThrowIfDisposed", BindingFlags.NonPublic | BindingFlags.Instance)
                                         ?? throw new ObjectDisposedException(userManager.GetType().Name);
            throwIfDisposed.Invoke(userManager, null);
        }

        private static IUserTelegramIdStore<TUser> GetTelegramIdStore<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            if (userManager
                .GetType()
                .GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetMethod?
                .Invoke(userManager, null) is not IUserTelegramIdStore<TUser> telegramIdStore)
            {
                throw new NotSupportedException("Store does not implement IUserTelegramIdStore<TUser>.");
            }
            return telegramIdStore;
        }

        private static ITelegramTokenStore GetTelegramTokenStore<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            if (userManager
                .GetType()
                .GetProperty("Store", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetMethod?
                .Invoke(userManager, null) is not ITelegramTokenStore telegramTokenStore)
            {
                throw new NotSupportedException("Store does not implement ITelegramTokenStore.");
            }
            return telegramTokenStore;
        }

        private static CancellationToken GetCancellationToken<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            if (userManager
                .GetType()
                .GetProperty("CancellationToken", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetMethod?
                .Invoke(userManager, null) is not CancellationToken cancellationToken)
            {
                throw new MissingMemberException("UserManager does not contain CancellationToken property.");
            }
            return cancellationToken;
        }

        private static IServiceProvider GetServiceProvider<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            if (userManager
                .GetType()
                .GetField("_services", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(userManager) is not IServiceProvider services)
            {
                throw new MissingMemberException("UserManager does not contain _services field.");
            }
            return services;
        }
    }
}
