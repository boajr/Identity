using Boa.TelegramBotService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Boa.Identity.Telegram
{
    public class TelegramBotHandler<TUser> : ITelegramBotHandler
        where TUser : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserTelegramIdStore<TUser> _userStore;
        private readonly UserManager<TUser> _userManager;

        public int Order => 500;

        /// <summary>
        /// Gets the <see cref="Microsoft.Extensions.Localization.IStringLocalizer"/> for localized strings.
        /// </summary>
        protected IStringLocalizer Localizer
        {
            get
            {
                _localizer ??= new IdentityStringLocalizer(_serviceProvider, "Boa.Identity.ResetPasswordService");
                return _localizer;
            }
        }
        private IStringLocalizer? _localizer;

        public TelegramBotHandler(IServiceProvider serviceProvider, IUserStore<TUser> userStore, UserManager<TUser> userManager)
        {
            if (userStore == null)
            {
                throw new ArgumentNullException(nameof(userStore));
            }

            if (userStore is not IUserPasswordStore<TUser>)
            {
                throw new NotSupportedException("Store does not implement IUserPasswordStore<TUser>.");
            }

            if (userStore is not IUserTelegramIdStore<TUser> userTelegramIdStore)
            {
                throw new NotSupportedException("Store does not implement IUserTelegramIdStore<TUser>.");
            }

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _userStore = userTelegramIdStore;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // verifico che sia un messaggio inviato in risposta ad un messaggio di controllo
            // generato dal bot. Identifico i messaggi di controllo perché iniziano con una
            // parentesi quadra.
            if (update.Type != UpdateType.Message || update.Message?.ReplyToMessage == null || update.Message.From == null
                || update.Message.ReplyToMessage.From?.Id != botClient.BotId || update.Message.ReplyToMessage.Text?[0] != '[')
                return false;

            int? pos = update.Message?.ReplyToMessage.Text.IndexOf("]");
            if (pos == null || pos == -1)
                return false;

            return (update.Message?.ReplyToMessage.Text[1..pos.Value]) switch
            {
                "RESETPWD" => await ResetPassword(botClient, update.Message.ReplyToMessage, update.Message, cancellationToken).ConfigureAwait(false),
                _ => false,
            };
        }

        private async Task<bool> ResetPassword(ITelegramBotClient botClient, Message request, Message response, CancellationToken cancellationToken)
        {
            // cancello entrambi i messaggi dalla chat
            await botClient.DeleteMessageAsync(response.Chat.Id, request.MessageId, cancellationToken).ConfigureAwait(false);
            await botClient.DeleteMessageAsync(response.Chat.Id, response.MessageId, cancellationToken).ConfigureAwait(false);

            // controllo che la differenza tra richiesta e risposta non superi le xxx ore (serve???)

            // controllo che gli id dei messaggi siano uno consecutivo all'altro (serve???)

            // cerco l'utente nella tabella
            TUser? user = await _userStore.FindByTelegramIdAsync(response.From!.Id, cancellationToken).ConfigureAwait(false);
            if (user == null)
            {
                // non dovrebbe mai capitare
                await botClient.SendTextMessageAsync(
                    chatId: response.Chat.Id,
                    text: Localizer["Unknown user"],
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                return true;
            }

            // creo un token e poi resetto la password, devo fare così perché la UserManager non ha
            // una funzione per forzare la sostituzione della password.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
            var result = await _userManager.ResetPasswordAsync(user, token, response.Text ?? "").ConfigureAwait(false);
            if (result.Succeeded)
            {
                await botClient.SendTextMessageAsync(
                    chatId: response.Chat.Id,
                    text: Localizer["Your password has been reset"],
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                return true;
            }

            // se non sono riuscito a resettare la password comunico al client il primo errore e
            // chiedo se si vuole riprovare.
            await botClient.SendTextMessageAsync(
                chatId: response.Chat.Id,
                text: "[RESETPWD] " +
                    result.Errors.First().Description + "\r\n\r\n" +
                    Localizer["Reply to this message with new password"],
                replyMarkup: new ForceReplyMarkup(),
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            return true;
        }
    }
}
