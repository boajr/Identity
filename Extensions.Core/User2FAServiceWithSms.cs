using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public abstract class User2FAServiceWithSms<TUser> : User2FAServiceWithTokenProvider<TUser>
    where TUser : class
{
    public User2FAServiceWithSms(IServiceProvider serviceProvider)
        : base(serviceProvider, TokenOptions.DefaultEmailProvider) { }

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
    /// <remarks>By default this calls into <see cref="ProcessSendToken"/>.</remarks>.
    protected override async Task<bool> ProcessSendTokenAsync(string token, UserManager<TUser> manager, TUser user)
    {
        var phone = await manager.GetPhoneNumberAsync(user);
        if (phone == null || !await manager.IsPhoneNumberConfirmedAsync(user))
        {
            return false;
        }

        await SendSmsAsync(
            phone,
            Localizer["Please, to authenticate use this code {0}", token]).ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// Sends a sms to the specified <paramref name="phoneNumber"/> as an asynchronous operation.
    /// </summary>
    /// <param name="email">The phone number to send sms.</param>
    /// <param name="message">The text of the sms.</param>
    /// <returns>A <see cref="Task"/> that completes when sms is sent.</returns>
    protected abstract Task SendSmsAsync(string phoneNumber, string message);
}
