using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace Boa.Identity.Telegram;

internal sealed class ResetPasswordServiceTelegramDataModel
{
    [Phone]
    [Display(Name = "Phone Number", Prompt = "+393xxyyyyyyy")]
    public string PhoneNumber { get; set; } = default!;
}

internal sealed class ResetPasswordServiceTelegram<TUser> : ResetPasswordService<ResetPasswordServiceTelegramDataModel>
    where TUser : class
{
    private readonly UserManager<TUser> _userManager;
    private readonly ITelegramBotClient _botClient;

    public sealed override string ServiceName => "Telegram";

    public sealed override string RequestMessage => "Insert your phone number";

    //public sealed override string LongMessage => "Insert your telegram phone number<br>to be contacted for new password";

    public sealed override string ConfirmationMessage => "Please check your telegram conversations to reset your password";

    public ResetPasswordServiceTelegram(IServiceProvider serviceProvider,
                                        IObjectModelValidator modelValidator,
                                        UserManager<TUser> userManager,
                                        Boa.TelegramBotService.TelegramBotService botClient)
        : base(serviceProvider, modelValidator)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
    }

    protected override async Task<bool> ProcessAsync(ResetPasswordServiceTelegramDataModel data)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        TUser? user = await _userManager.FindByPhoneNumberAsync(data.PhoneNumber).ConfigureAwait(false);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return true;
        }

        long? telegramId = await _userManager.GetTelegramIdAsync(user).ConfigureAwait(false);
        if (telegramId == null)
        {
            // Don't reveal that the user does not exist
            return true;
        }

        // remove old reset password message
        var msgIdText = await _userManager.GetTelegramTokenAsync(telegramId.Value, "[boaidentity]", "ResetPasswordMessage").ConfigureAwait(false);
        if (msgIdText != null && int.TryParse(msgIdText, out int msgId))
        {
            try
            {
                await _botClient.DeleteMessageAsync(telegramId, msgId).ConfigureAwait(false);
            }
            catch (ApiRequestException) { }
        }

        // send message to user Telegram Chat
        var msg = await _botClient.SendTextMessageAsync(
            chatId: telegramId,
            text: Localizer["Reply to this message with new password"],
            replyMarkup: new ForceReplyMarkup()
        ).ConfigureAwait(false);

        // save messageid to check response from user
        await _userManager.SetTelegramTokenAsync(telegramId.Value, "[boaidentity]", "ResetPasswordMessage", msg.MessageId.ToString()).ConfigureAwait(false);

        return true;
    }
}
