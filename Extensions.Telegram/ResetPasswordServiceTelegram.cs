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
    private readonly IUserTelegramIdStore<TUser> _userStore;
    private readonly ITelegramBotClient _botClient;

    public sealed override string ServiceName => "Telegram";

    public sealed override string RequestMessage => "Insert your phone number";

    //public sealed override string LongMessage => "Insert your telegram phone number<br>to be contacted for new password";

    public sealed override string ConfirmationMessage => "Please check your telegram conversations to reset your password";

    public ResetPasswordServiceTelegram(IServiceProvider serviceProvider,
                                        IObjectModelValidator modelValidator,
                                        IUserStore<TUser> userStore,
                                        Boa.TelegramBotService.TelegramBotService botClient)
        : base(serviceProvider, modelValidator)
    {
        if (userStore == null)
        {
            throw new ArgumentNullException(nameof(userStore));
        }

        if (userStore is not IUserTelegramIdStore<TUser> userTelegramIdStore)
        {
            throw new NotSupportedException("Store does not implement IUserTelegramIdStore<TUser>.");
        }

        _userStore = userTelegramIdStore;
        _botClient = botClient;
    }

    protected override async Task<bool> ProcessAsync(ResetPasswordServiceTelegramDataModel data)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        TUser? user = await _userStore.FindByPhoneNumberAsync(data.PhoneNumber, default);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return true;
        }

        long? telegramId = await _userStore.GetTelegramIdAsync(user, default);
        if (telegramId == null)
        {
            // Don't reveal that the user does not exist
            return true;
        }

        await _botClient.SendTextMessageAsync(
            chatId: telegramId,
            text: "[RESETPWD] " + Localizer["Reply to this message with new password"],
            replyMarkup: new ForceReplyMarkup()
        );

        return true;
    }
}
