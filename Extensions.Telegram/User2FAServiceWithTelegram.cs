using Microsoft.AspNetCore.Identity;
using Telegram.Bot;

namespace Boa.Identity.Telegram;

public class User2FAServiceWithTelegram<TUser> : User2FAServiceWithTokenProvider<TUser>
    where TUser : class
{
    private readonly TelegramBotService.TelegramBotService _botClient;

    public User2FAServiceWithTelegram(IServiceProvider serviceProvider, TelegramBotService.TelegramBotService botClient)
            : base(serviceProvider, TokenOptions.DefaultEmailProvider)
    {
        _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
    }

    public override bool NeedToSendToken => true;

    /// <summary>
    /// Asynchronously process the send token request after that the <paramref name="token"/> is generated.
    /// </summary>
    /// <param name="token">The generated token.</param>
    /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
    /// <param name="user">The user to send the token.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> that, when completed, returns <c>true</c> if the <paramref name="token"/>
    /// is sent, otherwise returns <c>false</c>.
    /// </returns>
    /// <remarks>By default this calls into <see cref="ProcessSendToken"/>.</remarks>
    protected override async Task<bool> ProcessSendTokenAsync(string token, UserManager<TUser> manager, TUser user)
    {
        long? telegramId = await manager.GetTelegramIdAsync(user).ConfigureAwait(false);
        if (telegramId == null)
        {
            return false;
        }

        await _botClient.SendTextMessageAsync(
            chatId: telegramId,
            text: Localizer["Please, to authenticate use this code {0}", token]
        ).ConfigureAwait(false);

        return true;
    }
}
