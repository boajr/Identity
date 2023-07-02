using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
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
    private readonly TelegramUserManager<TUser> _userManager;
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
        if (userManager == null)
        {
            throw new ArgumentNullException(nameof(userManager));
        }

        if (userManager is not TelegramUserManager<TUser> telegramUserManager)
        {
            throw new NotSupportedException("Store does not implement TelegramUserManager<TUser>.");
        }

        _userManager = telegramUserManager;
        _botClient = botClient;
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

        await _botClient.SendTextMessageAsync(
            chatId: telegramId,
            text: "[RESETPWD] " + Localizer["Reply to this message with new password"],
            replyMarkup: new ForceReplyMarkup()
        ).ConfigureAwait(false);

        return true;
    }
}
