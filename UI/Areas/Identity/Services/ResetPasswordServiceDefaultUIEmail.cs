using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Boa.Identity.UI.Services
{
    internal sealed class ResetPasswordServiceDefaultUIEmail<TUser> : ResetPasswordServiceEmail<TUser>
        where TUser : class
    {
        public ResetPasswordServiceDefaultUIEmail(IServiceProvider serviceProvider,
                                                  IObjectModelValidator modelValidator,
                                                  UserManager<TUser> userManager,
                                                  IEmailSender emailSender)
            : base(serviceProvider, modelValidator, userManager, emailSender) { }

        protected override string GetCallbackUrl(string token)
        {
            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}/Identity/Account/ResetPassword?code={token}";
        }
    }
}
