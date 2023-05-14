using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Boa.Identity.UI.Services
{
    internal sealed class ResetPasswordServiceDefaultUIEmail<TUser> : ResetPasswordServiceEmail<TUser>
        where TUser : class
    {
        private readonly IEmailSender _emailSender;

        public ResetPasswordServiceDefaultUIEmail(IServiceProvider serviceProvider,
                                                  IObjectModelValidator modelValidator,
                                                  UserManager<TUser> userManager,
                                                  IEmailSender emailSender)
            : base(serviceProvider, modelValidator, userManager)
        {
            _emailSender = emailSender;
        }

        protected override string GetCallbackUrl(string token)
        {
            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}/Identity/Account/ResetPassword?code={token}";
        }

        protected override Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return _emailSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
