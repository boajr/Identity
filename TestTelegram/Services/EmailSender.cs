using Microsoft.AspNetCore.Identity.UI.Services;

namespace TestTelegram.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            _logger.LogInformation("To: {To}\r\nSubject: {Subject}\r\n\r\n{message}", toEmail, subject, message);
            return Task.CompletedTask;
        }
    }
}
