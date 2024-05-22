using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public class User2FAServiceWithAuthenticator<TUser> : User2FAServiceWithTokenProvider<TUser>
    where TUser : class
{
    public User2FAServiceWithAuthenticator(IServiceProvider serviceProvider)
        : base(serviceProvider, TokenOptions.DefaultAuthenticatorProvider) { }

    public override string RequestMessage => @"Your login is protected with an authenticator app. Enter your authenticator code below";

    public override bool NeedToSendToken => false;

    protected override bool ProcessSendToken(string token, UserManager<TUser> manager, TUser user)
    {
        return true;
    }
}
