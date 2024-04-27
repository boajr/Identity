// nella signInManager.cs non c'è neanche un ConfigureAwait(false), quindi non li metto neanche io
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Boa.Identity
{
    /// <summary>
    /// Provides the APIs for user sign in.
    /// </summary>
    /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
    public class SignInManager<TUser> : Microsoft.AspNetCore.Identity.SignInManager<TUser>
        where TUser : class
    {
        /// <summary>
        /// Creates a new instance of <see cref="Boa.Identity.SignInManager{TUser}"/>.
        /// </summary>
        /// <param name="userManager">An instance of <see cref="UserManager"/> used to retrieve users from and persist users.</param>
        /// <param name="contextAccessor">The accessor used to access the <see cref="HttpContext"/>.</param>
        /// <param name="claimsFactory">The factory to use to create claims principals for a user.</param>
        /// <param name="optionsAccessor">The accessor used to access the <see cref="IdentityOptions"/>.</param>
        /// <param name="optionsAccessor">The accessor used to access the <see cref="BoaIdentityOptions"/>.</param>
        /// <param name="logger">The logger used to log messages, warnings and errors.</param>
        /// <param name="schemes">The scheme provider that is used enumerate the authentication schemes.</param>
        /// <param name="confirmation">The <see cref="IUserConfirmation{TUser}"/> used check whether a user account is confirmed.</param>
        public SignInManager(UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            IOptions<BoaIdentityOptions> boaOptionsAccessor,
            ILogger<Microsoft.AspNetCore.Identity.SignInManager<TUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<TUser> confirmation) :
            base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            BoaOptions = boaOptionsAccessor?.Value ?? new BoaIdentityOptions();
        }

        /// <summary>
        /// The <see cref="BoaIdentityOptions"/> used.
        /// </summary>
        public BoaIdentityOptions BoaOptions { get; set; }

        /// <summary>
        /// Check if the <paramref name="user"/> has two factor enabled.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// The task object representing the asynchronous operation containing true if the user has two factor enabled.
        /// </returns>
        public override async Task<bool> IsTwoFactorEnabledAsync(TUser user)
            => UserManager.SupportsUserTwoFactor
               &&
               (
                   BoaOptions.ForceTwoFactorAuthentication
                   ||
                   (
                       await UserManager.GetTwoFactorEnabledAsync(user) &&
                       (await UserManager.GetValidTwoFactorProvidersAsync(user)).Count > 0
                   )
               );




        public async Task<SignInResult> BoaTwoFactorSignInWithTokenAsync(IUser2FAServiceWithToken<TUser> service, string code, bool isPersistent, bool rememberClient, bool rememberService)
        {
            var twoFactorInfo = await RetrieveTwoFactorInfoAsync();
            if (twoFactorInfo == null)
            {
                return SignInResult.Failed;
            }

            if (twoFactorInfo.GetType().GetProperty("User")?.GetValue(twoFactorInfo) is not TUser user)
            {
                return SignInResult.Failed;
            }
            var error = await PreSignInCheck(user);
            if (error != null)
            {
                return error;
            }

            if (await service.ValidateTokenAsync(code, UserManager, user))
            {
                var result = await DoTwoFactorSignInAsync(user, twoFactorInfo, isPersistent, rememberClient);
                // Se richiesto salvo il servizio di autenticazione usato
                if (result.Succeeded && rememberService)
                {
                    await UserManager.SetTwoFactorProviderAsync(user, service.ServiceName);
                }
                return result;
            }
            // If the token is incorrect, record the failure which also may cause the user to be locked out
            if (UserManager.SupportsUserLockout)
            {
                var incrementLockoutResult = await UserManager.AccessFailedAsync(user) ?? IdentityResult.Success;
                if (!incrementLockoutResult.Succeeded)
                {
                    // Return the same failure we do when resetting the lockout fails after a correct two factor code.
                    // This is currently redundant, but it's here in case the code gets copied elsewhere.
                    return SignInResult.Failed;
                }

                if (await UserManager.IsLockedOutAsync(user))
                {
                    return await LockedOut(user);
                }
            }
            return SignInResult.Failed;
        }

        private async Task<object?> RetrieveTwoFactorInfoAsync()
        {
            // BaseType non può essere nullo visto che la classe è derivata dalla Microsoft.AspNetCore.Identity.SignInManager
            MethodInfo retrieveTwoFactorInfoAsync = GetType().BaseType?.GetMethod("RetrieveTwoFactorInfoAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                                         ?? throw new MissingMethodException(GetType().BaseType?.Name + " do not implement private method RetrieveTwoFactorInfoAsync");
            if (retrieveTwoFactorInfoAsync.Invoke(this, null) is not Task task)
            {
                throw new InvalidOperationException("RetrieveTwoFactorInfoAsync return value is not a Task");
            }

            await task;
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }

        private Task<SignInResult> DoTwoFactorSignInAsync(TUser user, object twoFactorInfo, bool isPersistent, bool rememberClient)
        {
            // BaseType non può essere nullo visto che la classe è derivata dalla Microsoft.AspNetCore.Identity.SignInManager
            MethodInfo doTwoFactorSignInAsync = GetType().BaseType?.GetMethod("DoTwoFactorSignInAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                                         ?? throw new MissingMethodException(GetType().BaseType?.Name + " do not implement private method DoTwoFactorSignInAsync");
            if (doTwoFactorSignInAsync.Invoke(this, [user, twoFactorInfo, isPersistent, rememberClient]) is not Task<SignInResult> task)
            {
                throw new InvalidOperationException("RetrieveTwoFactorInfoAsync return value is not a Task<SignInResult>");
            }

            return task;
        }
    }
}
