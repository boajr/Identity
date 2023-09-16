using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public abstract class User2FAServiceWithEmail<TUser> : User2FAServiceWithTokenProvider<TUser>
    where TUser : class
{
    public User2FAServiceWithEmail(IServiceProvider serviceProvider)
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
        var email = await manager.GetEmailAsync(user);
        if (email == null || !await manager.IsEmailConfirmedAsync(user))
        {
            return false;
        }

        await SendEmailAsync(
            email,
            Localizer["Two Factor Authentication Token"],
            Localizer["Please, to authenticate use this code {0}", token]).ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// Sends an email to the specified <paramref name="email"/> address as an asynchronous operation.
    /// </summary>
    /// <param name="email">The email address to send email.</param>
    /// <param name="subject">The subject of the eamil.</param>
    /// <param name="htmlMessage">The body of the email in html format.</param>
    /// <returns>A <see cref="Task"/> that completes when mail is sent.</returns>
    protected abstract Task SendEmailAsync(string email, string subject, string htmlMessage);
}
