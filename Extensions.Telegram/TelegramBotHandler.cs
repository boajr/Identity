using Boa.TelegramBotService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
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
        private readonly TelegramUserManager<TUser> _userManager;

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

        public TelegramBotHandler(IServiceProvider serviceProvider, UserManager<TUser> userManager)
        {
            if (userManager == null)
            {
                throw new ArgumentNullException(nameof(userManager));
            }

            if (userManager is not TelegramUserManager<TUser> telegramUserManager)
            {
                throw new NotSupportedException("Store does not implement TelegramUserManager<TUser>.");
            }

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _userManager = telegramUserManager;
        }

        public async Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // verifico che sia un messaggio inviato in risposta ad un messaggio di controllo
            // generato dal bot. Identifico i messaggi di controllo perché iniziano con una
            // parentesi quadra.
            if (update.Type != UpdateType.Message || update.Message?.ReplyToMessage == null || update.Message.From == null
                || update.Message.ReplyToMessage.From?.Id != botClient.BotId || update.Message.ReplyToMessage.Text?[0] != '[')
                return false;

            Console.WriteLine(update.Message.From.LanguageCode);

            int? pos = update.Message?.ReplyToMessage.Text.IndexOf("]");
            if (pos == null || pos == -1)
                return false;

            return (update.Message?.ReplyToMessage.Text[1..pos.Value]) switch
            {
                "UNKNWUSR" => await RegisterUser(botClient, update.Message.ReplyToMessage, update.Message, cancellationToken).ConfigureAwait(false),
                "RESETPWD" => await ResetPassword(botClient, update.Message.ReplyToMessage, update.Message, cancellationToken).ConfigureAwait(false),
                _ => false,
            };
        }

        [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public class ForceReplyKeyboardMarkup : ReplyMarkupBase //ReplyKeyboardMarkup
        {
            /// <summary>
            /// Shows reply interface to the user, as if they manually selected the bot’s message and tapped 'Reply'
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public bool ForceReply => true;

            //public ForceReplyKeyboardMarkup(IEnumerable<IEnumerable<KeyboardButton>> keyboard): base (keyboard)
            //{
            //}
        }


        private async Task<bool> RegisterUser(ITelegramBotClient botClient, Message request, Message response, CancellationToken cancellationToken)
        {
            //await botClient.DeleteMessageAsync(response.Chat.Id, request.MessageId, cancellationToken).ConfigureAwait(false);
            await botClient.DeleteMessageAsync(response.Chat.Id, response.MessageId, cancellationToken).ConfigureAwait(false);

            if (response.Type == MessageType.Contact && response.Contact != null)
            {
                if (response.Contact.UserId != response.From!.Id)
                {
                    IReplyMarkup markup = new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton[] { KeyboardButton.WithRequestContact(Localizer["Send\r\nCONTACT CARD"]) },
                        new KeyboardButton[] { new KeyboardButton("[UNKNWUSR] " + Localizer["Cancel"]) }
                    });

                    await botClient.SendTextMessageAsync(
                        chatId: response.Chat.Id,
                        text: "[UNKNWUSR] " + Localizer["This isn't your contact card!\r\nPlease post the right one"],
                        replyMarkup: markup,
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);


                    //await botClient.SendTextMessageAsync(
                    //    chatId: response.Chat.Id,
                    //    text: "[RESETPWD] " +
                    //        result.Errors.First().Description + "\r\n\r\n" +
                    //        Localizer["Reply to this message with new password"],
                    //    replyMarkup: new ForceReplyMarkup(),
                    //    cancellationToken: cancellationToken
                    //).ConfigureAwait(false);



                }

            }






            return true;
        }

        private async Task<bool> ResetPassword(ITelegramBotClient botClient, Message request, Message response, CancellationToken cancellationToken)
        {
            // cancello entrambi i messaggi dalla chat
            //await botClient.DeleteMessageAsync(response.Chat.Id, request.MessageId, cancellationToken).ConfigureAwait(false);
            await botClient.DeleteMessageAsync(response.Chat.Id, response.MessageId, cancellationToken).ConfigureAwait(false);

            // controllo che la differenza tra richiesta e risposta non superi le xxx ore (serve???)

            // controllo che gli id dei messaggi siano uno consecutivo all'altro (serve???)

            // cerco l'utente nella tabella
            TUser? user = await _userManager.FindByTelegramIdAsync(response.From!.Id, true, botClient).ConfigureAwait(false);
            if (user == null)
            {
                // non dovrebbe mai capitare, ma in ogni caso non faccio nulla perché chiedo già
                // allo user manager di inviare la richiesta di identificazione dell'utente
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
