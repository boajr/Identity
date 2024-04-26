using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public class User2FAServiceWithSms<TUser> : User2FAServiceWithTokenProvider<TUser>
    where TUser : class
{
    private readonly ISmsSender _smsSender;

    public User2FAServiceWithSms(IServiceProvider serviceProvider, ISmsSender smsSender)
        : base(serviceProvider, TokenOptions.DefaultPhoneProvider)
    {
        _smsSender = smsSender;
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
    protected override async Task<bool> ProcessSendTokenAsync(string token, UserManager<TUser> manager, TUser user)
    {
        var phone = await manager.GetPhoneNumberAsync(user).ConfigureAwait(false);
        if (phone == null || !await manager.IsPhoneNumberConfirmedAsync(user).ConfigureAwait(false))
        {
            return false;
        }

        await _smsSender.SendSmsAsync(
            phone,
            Localizer["Please, to authenticate use this code {0}", token]).ConfigureAwait(false);

        return true;
    }
}
