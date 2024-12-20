﻿using Boa.TelegramBotService;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Boa.Identity.Telegram
{
    public class TelegramBotHandler<TUser> : ITelegramBotHandler
        where TUser : class
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<TUser> _userManager;

        public int Order => 500;

        /// <summary>
        /// Gets the <see cref="IStringLocalizer"/> for localized strings.
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
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        private readonly string[] Actions = ["RegisterUserMessage", "ResetPasswordMessage"];

        public async Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // se non è un messaggio, non faccio niente (per il momento)
            if (update.Type != UpdateType.Message || update.Message?.From == null)
                return false;

            // cambia la lingua di risposta del bot
            if (update.Message.From.LanguageCode != null && update.Message.From.LanguageCode != CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            {
                var lang = CultureInfo.GetCultureInfo(update.Message.From.LanguageCode);
                if (lang != null)
                {
                    CultureInfo.CurrentUICulture = lang;
                }
            }

            // verifico se ho azioni pending per l'utente
            long chatId = update.Message.Chat.Id;
            var tokens = await _userManager.GetAllTelegramTokensAsync(chatId, "[boaidentity]");

            // salvo su una variabile locale l'id del messaggio risposto
            int? replyId = update.Message.ReplyToMessage?.MessageId;

            // cancello tutte le azioni pending (perché al primo messaggio risposto vengono annullate)
            bool retVal = false;
            foreach (var (name, value) in tokens)
            {
                if (!Actions.Contains(name))
                    continue;

                if (!int.TryParse(value, out var msgId))
                    continue;

                // elimina la riga dal DB
                await _userManager.RemoveTelegramTokenAsync(chatId, "[boaidentity]", name);

                // elimina la riga dalla chat
                await DeleteMessageNoExceptionAsync(botClient, chatId, msgId, cancellationToken).ConfigureAwait(false);

                // verifico se il messaggio è la risposta a un'azione
                if (msgId == replyId)
                {
                    switch (name)
                    {
                        case "RegisterUserMessage":
                            retVal = await RegisterUser(botClient, update.Message, null, cancellationToken).ConfigureAwait(false);
                            break;

                        case "ResetPasswordMessage":
                            retVal = await ResetPassword(botClient, update.Message, cancellationToken).ConfigureAwait(false);
                            break;
                    }
                }

                // verifico se il messaggio è il messaggio immediatamente successivo a un'azione
                if (msgId + 1 == update.Message.MessageId)
                {
                    switch (name)
                    {
                        case "RegisterUserMessage":
                            // potrebbe essere l'annulla, in questo caso cancello il messaggio
                            if (update.Message.Text == Localizer["Cancel"])
                            {
                                retVal = await RegisterUser(botClient, update.Message, null, cancellationToken).ConfigureAwait(false);
                            }
                            break;

                            //case "ResetPasswordMessage":
                            //    retVal = await ResetPassword(botClient, update.Message, cancellationToken).ConfigureAwait(false);
                            //    break;
                    }
                }
            }

            // arrivo qui se il messaggio non è una risposta ad un'azione, quindi verifico che non
            // sia l'inizio di una richiesta di registrazione (è il comando /register)
            if (!retVal && update.Message.Type == MessageType.Text && update.Message.Text != null)
            {
                var command = update.Message.Text.Split(' ').First();

                if (command == Localizer["/register"])
                {
                    retVal = await RegisterUser(botClient, update.Message, Localizer["Please post your contact card to be identified"], cancellationToken).ConfigureAwait(false);
                }
            }

            return retVal;
        }

        private static async Task DeleteMessageNoExceptionAsync(ITelegramBotClient botClient, ChatId chatId, int messageId, CancellationToken cancellationToken)
        {
            try
            {
                await botClient.DeleteMessage(chatId, messageId, cancellationToken).ConfigureAwait(false);
            }
            catch (ApiRequestException) { }
        }

        private async Task<bool> RegisterUser(ITelegramBotClient botClient, Message response, string? text, CancellationToken cancellationToken)
        {
            await DeleteMessageNoExceptionAsync(botClient, response.Chat.Id, response.MessageId, cancellationToken).ConfigureAwait(false);

            bool retry = true;

            if (response.Type == MessageType.Contact && response.Contact != null)
            {
                if (response.Contact.UserId != response.From!.Id)
                {
                    text = Localizer["This isn't your contact card!\r\nPlease post the right one"];
                }
                else if (string.IsNullOrWhiteSpace(response.Contact.PhoneNumber))
                {
                    text = Localizer["This contact card does not contain the phone number!\r\nPlease post one with the correct value"];
                }
                else
                {
                    // cerca nella tabella degli utenti il numero di telefono (Telegram li passa senza il + iniziale)
                    var user = await _userManager.FindByPhoneNumberAsync("+" + response.Contact.PhoneNumber);
                    if (user == null)
                    {
                        text = Localizer["Your phone number is not in the user list!"];
                        retry = false;
                    }
                    else
                    {
                        await _userManager.SetTelegramIdAsync(user, response.Contact.UserId);
                        text = Localizer["User updated successfully"];
                        retry = false;
                    }
                }
            }
            else
            {
                // se arrivo qui, o è il comando \register o un generico messaggio di testo. Se non
                // è il comando \register, considero la risposta la pressione del tasto "Cancel"
                if (text == null)
                {
                    retry = false;
                }
            }

            // invio un messaggio all'utente o per chiedergli nuovamente la scheda di contatto o per nascondergli la tastiera
            var msg = await botClient.SendMessage(
                chatId: response.Chat.Id,
                text: text ?? "Remove keyboard",
                replyMarkup: retry ?
                    new ReplyKeyboardMarkup(new KeyboardButton[][] {
                            [ KeyboardButton.WithRequestContact(Localizer["Send\r\nCONTACT CARD"]) ],
                            [ new KeyboardButton(Localizer["Cancel"]) ]
                    }) : new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            if (retry)
            {
                // Se invio nuovamente la richiesta all'utente, la registro per poter processare la
                // risposta che mi darà
                await _userManager.SetTelegramTokenAsync(response.Chat.Id, "[boaidentity]", "RegisterUserMessage", msg.MessageId.ToString()).ConfigureAwait(false);
            }
            else if (text == null)
            {
                // Se invece non ho un testo da visualizzare all'utente, vuol dire che il messaggio
                // l'ho inviato solo per nascondere la tastiera, e quindi non serve che l'utente lo
                // legga
                await DeleteMessageNoExceptionAsync(botClient, response.Chat.Id, msg.MessageId, cancellationToken).ConfigureAwait(false);
            }
            return true;
        }

        private async Task<bool> ResetPassword(ITelegramBotClient botClient, Message response, CancellationToken cancellationToken)
        {
            // cancello il messaggio contenete la password
            await DeleteMessageNoExceptionAsync(botClient, response.Chat.Id, response.MessageId, cancellationToken).ConfigureAwait(false);

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
                await botClient.SendMessage(
                    chatId: response.Chat.Id,
                    text: Localizer["Your password has been reset"],
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                return true;
            }

            // se non sono riuscito a resettare la password comunico al client il primo errore e
            // chiedo se si vuole riprovare.
            var msg = await botClient.SendMessage(
                chatId: response.Chat.Id,
                text: result.Errors.First().Description + "\r\n\r\n" +
                    Localizer["Reply to this message with new password"],
                replyMarkup: new ForceReplyMarkup(),
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            // save messageid to check response from user
            await _userManager.SetTelegramTokenAsync(response.Chat.Id, "[boaidentity]", "ResetPasswordMessage", msg.MessageId.ToString()).ConfigureAwait(false);

            return true;
        }
    }
}
