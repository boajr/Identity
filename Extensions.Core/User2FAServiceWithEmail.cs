﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Boa.Identity;

public class User2FAServiceWithEmail<TUser> : User2FAServiceWithTokenProvider<TUser>
    where TUser : class
{
    private readonly IEmailSender _emailSender;

    public User2FAServiceWithEmail(IServiceProvider serviceProvider, IEmailSender emailSender)
        : base(serviceProvider, TokenOptions.DefaultEmailProvider)
    {
        _emailSender = emailSender;
    }

    public override string RequestMessage => @"An authenticator code was sent to your email. Enter that code below";

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
        var email = await manager.GetEmailAsync(user).ConfigureAwait(false);
        if (email == null || !await manager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
        {
            return false;
        }

        await _emailSender.SendEmailAsync(
            email,
            Localizer["Two Factor Authentication Token"],
            Localizer["Please, to authenticate use this code {0}", token]).ConfigureAwait(false);

        return true;
    }
}
