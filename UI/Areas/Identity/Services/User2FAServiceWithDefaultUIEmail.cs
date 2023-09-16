using Microsoft.AspNetCore.Identity.UI.Services;

namespace Boa.Identity.UI.Services
{
    internal sealed class User2FAServiceWithDefaultUIEmail<TUser> : User2FAServiceWithEmail<TUser>
        where TUser : class
    {
        private readonly IEmailSender _emailSender;

        public User2FAServiceWithDefaultUIEmail(IServiceProvider serviceProvider,
                                                IEmailSender emailSender)
            : base(serviceProvider)
        {
            _emailSender = emailSender;
        }

        public override bool NeedToSendToken => true;

        protected override Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return _emailSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
