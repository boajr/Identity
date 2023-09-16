namespace Boa.Identity.UI.Services
{
    internal sealed class User2FAServiceWithDefaultUISms<TUser> : User2FAServiceWithSms<TUser>
        where TUser : class
    {
        private readonly ISmsSender _smsSender;

        public User2FAServiceWithDefaultUISms(IServiceProvider serviceProvider,
                                              ISmsSender smsSender)
            : base(serviceProvider)
        {
            _smsSender = smsSender;
        }

        public override bool NeedToSendToken => true;

        protected override Task SendSmsAsync(string phoneNumber, string message)
        {
            return _smsSender.SendSmsAsync(phoneNumber, message);
        }
    }
}
