using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Microsoft.Extensions.Localization;
using System.Threading;

namespace Boa.Identity.Telegram
{
    /// <summary>
    /// Provides the APIs for managing user in a persistence store.
    /// </summary>
    /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
    public class TelegramUserManager<TUser> : UserManager<TUser> where TUser : class
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Constructs a new instance of <see cref="TelegramUserManager{TUser}"/>.
        /// </summary>
        /// <param name="store">The persistence store the manager will operate over.</param>
        /// <param name="optionsAccessor">The accessor used to access the <see cref="IdentityOptions"/>.</param>
        /// <param name="passwordHasher">The password hashing implementation to use when saving passwords.</param>
        /// <param name="userValidators">A collection of <see cref="IUserValidator{TUser}"/> to validate users against.</param>
        /// <param name="passwordValidators">A collection of <see cref="IPasswordValidator{TUser}"/> to validate passwords against.</param>
        /// <param name="keyNormalizer">The <see cref="ILookupNormalizer"/> to use when generating index keys for users.</param>
        /// <param name="errors">The <see cref="Microsoft.AspNetCore.Identity.IdentityErrorDescriber"/> used to provider error messages.</param>
        /// <param name="services">The <see cref="IServiceProvider"/> used to resolve services.</param>
        /// <param name="logger">The logger used to log messages, warnings and errors.</param>
        public TelegramUserManager(IUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            Microsoft.AspNetCore.Identity.IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<TUser>> logger) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _services = services;
        }



        /// <summary>
        /// Gets a flag indicating whether the backing user store supports user telegram ids.
        /// </summary>
        /// <value>
        /// true if the backing user store supports user telegram ids, otherwise false.
        /// </value>
        public virtual bool SupportsUserTelegramId
        {
            get
            {
                ThrowIfDisposed();
                return Store is IUserTelegramIdStore<TUser>;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether the backing user store supports Telegram tokens.
        /// </summary>
        /// <value>
        /// true if the backing user store supports Telegram tokens, otherwise false.
        /// </value>
        public virtual bool SupportsTelegramTokens
        {
            get
            {
                ThrowIfDisposed();
                return Store is ITelegramTokenStore;
            }
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
        public virtual async Task<TUser?> FindByPhoneNumberAsync(string phoneNumber)
        {
            ThrowIfDisposed();
            var store = GetTelegramIdStore();
            if (phoneNumber == null)
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            var user = await store.FindByPhoneNumberAsync(phoneNumber, CancellationToken).ConfigureAwait(false);

            // Need to potentially check all keys
            if (user == null && Options.Stores.ProtectPersonalData)
            {
                var keyRing = _services.GetService<ILookupProtectorKeyRing>();
                var protector = _services.GetService<ILookupProtector>();
                if (keyRing != null && protector != null)
                {
                    foreach (var key in keyRing.GetAllKeyIds())
                    {
                        var oldKey = protector.Protect(key, phoneNumber);
                        user = await store.FindByPhoneNumberAsync(oldKey, CancellationToken).ConfigureAwait(false);
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
        public virtual async Task<TUser?> FindByTelegramIdAsync(long telegramId, bool askToRegister = false, ITelegramBotClient? botClient = null)
        {
            ThrowIfDisposed();
            var store = GetTelegramIdStore();

            var user = await store.FindByTelegramIdAsync(telegramId, CancellationToken).ConfigureAwait(false);

            //capire se è possibile proteggere gli id!!!
            //capire se posso aggiungere le opzioni identityOptions.User.RequireUniqueTelegramId e identityOptions.User.RequireUniquePhoneNumber

            // Need to potentially check all keys
            //if (user == null && Options.Stores.ProtectPersonalData)
            //{
            //    var keyRing = _services.GetService<ILookupProtectorKeyRing>();
            //    var protector = _services.GetService<ILookupProtector>();
            //    if (keyRing != null && protector != null)
            //    {
            //        foreach (var key in keyRing.GetAllKeyIds())
            //        {
            //            var oldKey = protector.Protect(key, telegramId);
            //            user = await store.FindByTelegramIdAsync(oldKey, CancellationToken).ConfigureAwait(false);
            //            if (user != null)
            //            {
            //                return user;
            //            }
            //        }
            //    }
            //}

            if (user == null && askToRegister && botClient != null)
            {
                IStringLocalizer localizer = new IdentityStringLocalizer(_services, "Boa.Identity.ResetPasswordService");

                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { KeyboardButton.WithRequestContact(localizer["Send\r\nCONTACT CARD"]) },
                    new KeyboardButton[] { new KeyboardButton(localizer["Cancel"]) }
                });

                await SetAuthenticationTokenAsync(user, "MyApp", "RefreshToken", "BOH!!!").ConfigureAwait(false);

                await botClient.SendTextMessageAsync(
                    chatId: telegramId,
                    text: "[UNKNWUSR] " + localizer["Unknown user!\r\nPlease post your contact card to be identified"],
                    replyMarkup: keyboard
                ).ConfigureAwait(false);

                //await botClient.SendTextMessageAsync(
                //    chatId: telegramId,
                //    text: "[UNKNWUSR]" + localizer["Unknown user!\r\nPlease post your contact card to be identified"],
                //    replyMarkup: new ForceReplyMarkup()
                //).ConfigureAwait(false);
            }

            return user;
        }

        /// <summary>
        /// Gets the telegram id, if any, for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose telegram id should be retrieved.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telegram id, if any.</returns>
        public virtual async Task<long?> GetTelegramIdAsync(TUser user)
        {
            ThrowIfDisposed();
            var store = GetTelegramIdStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return await store.GetTelegramIdAsync(user, CancellationToken).ConfigureAwait(false);
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
        public virtual async Task<IdentityResult> SetTelegramIdAsync(TUser user, long? telegramId)
        {
            ThrowIfDisposed();
            var store = GetTelegramIdStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await store.SetTelegramIdAsync(user, telegramId, CancellationToken).ConfigureAwait(false);
            return await UpdateSecurityStampAsync(user).ConfigureAwait(false);
        }







        /// <summary>
        /// Returns a Telegram token for a user.
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <returns>The authentication token for a user</returns>
        public virtual Task<string?> GetTelegramTokenAsync(long telegramId, string loginProvider, string tokenName)
        {
            ThrowIfDisposed();
            var store = GetTelegramTokenStore();
            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (tokenName == null)
            {
                throw new ArgumentNullException(nameof(tokenName));
            }

            return store.GetTelegramTokenAsync(telegramId, loginProvider, tokenName, CancellationToken);
        }

        /// <summary>
        /// Sets a Telegram token for a user.
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <param name="tokenValue">The value of the token.</param>
        /// <returns>Whether the user was successfully updated.</returns>
        public virtual async Task<IdentityResult> SetTelegramTokenAsync(long telegramId, string loginProvider, string tokenName, string? tokenValue)
        {
            ThrowIfDisposed();
            var store = GetTelegramTokenStore();
            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (tokenName == null)
            {
                throw new ArgumentNullException(nameof(tokenName));
            }

            // REVIEW: should updating any tokens affect the security stamp?
            return await store.SaveChangesAsync(CancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove a Telegram token for a user.
        /// </summary>
        /// <param name="telegramId"></param>
        /// <param name="loginProvider">The authentication scheme for the provider the token is associated with.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <returns>Whether a token was removed.</returns>
        public virtual async Task<IdentityResult> RemoveTelegramTokenAsync(long telegramId, string loginProvider, string tokenName)
        {
            ThrowIfDisposed();
            var store = GetTelegramTokenStore();
            if (loginProvider == null)
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (tokenName == null)
            {
                throw new ArgumentNullException(nameof(tokenName));
            }

            await store.RemoveTelegramTokenAsync(telegramId, loginProvider, tokenName, CancellationToken).ConfigureAwait(false);
            return await store.SaveChangesAsync(CancellationToken).ConfigureAwait(false);
        }

        private IUserTelegramIdStore<TUser> GetTelegramIdStore()
        {
            if (Store is not IUserTelegramIdStore<TUser> telegramIdStore)
            {
                throw new NotSupportedException("Store does not implement IUserTelegramIdStore<TUser>.");
            }
            return telegramIdStore;
        }

        private ITelegramTokenStore GetTelegramTokenStore()
        {
            if (Store is not ITelegramTokenStore telegramTokenStore)
            {
                throw new NotSupportedException("Store does not implement ITelegramTokenStore.");
            }
            return telegramTokenStore;
        }
    }
}
