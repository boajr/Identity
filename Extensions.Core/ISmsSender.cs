namespace Boa.Identity;

/// <summary>
/// This API supports the ASP.NET Core Identity infrastructure and is not intended to be used as a general purpose
/// sms abstraction. It should be implemented by the application so the Identity infrastructure can send confirmation and password reset sms.
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// This API supports the ASP.NET Core Identity infrastructure and is not intended to be used as a general purpose
    /// sms abstraction. It should be implemented by the application so the Identity infrastructure can send confirmation and password reset sms.
    /// </summary>
    Task SendSmsAsync(string phoneNumber, string message);
}
