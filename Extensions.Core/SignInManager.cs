using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        /// Creates a new instance of <see cref="SignInManager{TUser}"/>.
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
            ILogger<SignInManager<TUser>> logger,
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
    }
}
