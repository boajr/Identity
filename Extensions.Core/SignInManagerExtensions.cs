using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
    public static async Task<SignInResult> TwoFactorSignInWithTokenAsync<TUser>(this SignInManager<TUser> signInManager, IUser2FAServiceWithToken<TUser> service, string code, bool isPersistent, bool rememberClient, bool rememberService) where TUser : class
    {
        var twoFactorInfo = await RetrieveTwoFactorInfoAsync(signInManager).ConfigureAwait(false);
        if (twoFactorInfo == null)
        {
            return SignInResult.Failed;
        }
        if (twoFactorInfo.GetType().GetProperty("UserId")?.GetValue(twoFactorInfo) is not string userId)
        {
            return SignInResult.Failed;
        }

        var user = await signInManager.UserManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user == null)
        {
            return SignInResult.Failed;
        }

        var error = await PreSignInCheck(signInManager, user).ConfigureAwait(false);
        if (error != null)
        {
            return error;
        }
        if (await service.ValidateTokenAsync(code, signInManager.UserManager, user).ConfigureAwait(false))
        {
            var result = await DoTwoFactorSignInAsync(signInManager, user, twoFactorInfo, isPersistent, rememberClient).ConfigureAwait(false);
            // Se richiesto salvo il servizio di autenticazione usato
            if (result.Succeeded && rememberService)
            {
                await signInManager.UserManager.SetTwoFactorProviderAsync(user, service.ServiceName).ConfigureAwait(false); 
            }
            return result;
        }
        // If the token is incorrect, record the failure which also may cause the user to be locked out
        if (signInManager.UserManager.SupportsUserLockout)
        {
            var incrementLockoutResult = await signInManager.UserManager.AccessFailedAsync(user).ConfigureAwait(false) ?? IdentityResult.Success;
            if (!incrementLockoutResult.Succeeded)
            {
                // Return the same failure we do when resetting the lockout fails after a correct two factor code.
                // This is currently redundant, but it's here in case the code gets copied elsewhere.
                return SignInResult.Failed;
            }
        }
        return SignInResult.Failed;
    }

    private static async Task<object?> RetrieveTwoFactorInfoAsync<TUser>(SignInManager<TUser> signInManager) where TUser : class
    {
        if (signInManager
            .GetType()
            .GetMethod("RetrieveTwoFactorInfoAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(signInManager, null) is not Task task)
        {
            return null;
        }
        
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static Task<SignInResult?> PreSignInCheck<TUser>(SignInManager<TUser> signInManager, TUser user) where TUser : class
    {
        if (signInManager
            .GetType()
            .GetMethod("PreSignInCheck", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(signInManager, new object[] { user }) is not Task<SignInResult?> preSignInCheck)
        {
            return Task.FromResult<SignInResult?>(SignInResult.Failed);
        }
        return preSignInCheck;
    }

    private static Task<SignInResult> DoTwoFactorSignInAsync<TUser>(SignInManager<TUser> signInManager, TUser user, object twoFactorInfo, bool isPersistent, bool rememberClient) where TUser : class
    {
        if (signInManager
            .GetType()
            .GetMethod("DoTwoFactorSignInAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(signInManager, new object[] { user, twoFactorInfo, isPersistent, rememberClient }) is not Task<SignInResult> doTwoFactorSignInAsync)
        {
            return Task.FromResult<SignInResult>(SignInResult.Failed);
        }
        return doTwoFactorSignInAsync;
    }
}
