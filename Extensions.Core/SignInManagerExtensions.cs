// nella signInManager.cs non c'è neanche un ConfigureAwait(false), quindi non li metto neanche io
using Microsoft.AspNetCore.Identity;
using System.Reflection;

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
        var twoFactorInfo = await RetrieveTwoFactorInfoAsync(signInManager);
        if (twoFactorInfo == null)
        {
            return SignInResult.Failed;
        }

        if (twoFactorInfo.GetType().GetProperty("User")?.GetValue(twoFactorInfo) is not TUser user)
        {
            return SignInResult.Failed;
        }
        var error = await PreSignInCheck(signInManager, user);
        if (error != null)
        {
            return error;
        }

        if (await service.ValidateTokenAsync(code, signInManager.UserManager, user))
        {
            var result = await DoTwoFactorSignInAsync(signInManager, user, twoFactorInfo, isPersistent, rememberClient);
            // Se richiesto salvo il servizio di autenticazione usato
            if (result.Succeeded && rememberService)
            {
                await signInManager.UserManager.SetTwoFactorProviderAsync(user, service.ServiceName); 
            }
            return result;
        }
        // If the token is incorrect, record the failure which also may cause the user to be locked out
        if (signInManager.UserManager.SupportsUserLockout)
        {
            var incrementLockoutResult = await signInManager.UserManager.AccessFailedAsync(user) ?? IdentityResult.Success;
            if (!incrementLockoutResult.Succeeded)
            {
                // Return the same failure we do when resetting the lockout fails after a correct two factor code.
                // This is currently redundant, but it's here in case the code gets copied elsewhere.
                return SignInResult.Failed;
            }

            if (await signInManager.UserManager.IsLockedOutAsync(user))
            {
                return await LockedOut(signInManager, user);
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
        
        await task;
        return task.GetType().GetProperty("Result")?.GetValue(task);
    }

    private static Task<SignInResult> LockedOut<TUser>(SignInManager<TUser> signInManager, TUser user) where TUser : class
    {
        if (signInManager
            .GetType()
            .GetMethod("LockedOut", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(signInManager, new object[] { user }) is not Task<SignInResult> lochedOut)
        {
            return Task.FromResult<SignInResult>(SignInResult.Failed);
        }
        return lochedOut;
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
