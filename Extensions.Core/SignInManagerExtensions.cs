using Microsoft.AspNetCore.Identity;

namespace Boa.Identity;

public static class SignInManagerExtensions
{
    /// <summary>
    /// Validates the two factor sign in code and creates and signs in the user, as an asynchronous operation.
    /// </summary>
    /// <param name="provider">The two factor authentication provider to validate the code against.</param>
    /// <param name="code">The two factor authentication code to validate.</param>
    /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
    /// <param name="rememberClient">Flag indicating whether the current browser should be remember, suppressing all further
    /// two factor authentication prompts.</param>
    /// <returns>The task object representing the asynchronous operation containing the <see name="SignInResult"/>
    /// for the sign-in attempt.</returns>
    public static Task<SignInResult> TwoFactorSignInWithTokenAsync<TUser>(this Microsoft.AspNetCore.Identity.SignInManager<TUser> signInManager, IUser2FAServiceWithToken<TUser> service, string code, bool isPersistent, bool rememberClient, bool rememberService) where TUser : class
    {
        if (signInManager is not Boa.Identity.SignInManager<TUser> boaSignInManager)
        {
            throw new InvalidOperationException(signInManager.GetType().Name + " is not derived from Boa.Identity.SignInManager<" + typeof(TUser).Name + ">.");
        }

        return boaSignInManager.BoaTwoFactorSignInWithTokenAsync(service, code, isPersistent, rememberClient, rememberService);
    }
}
