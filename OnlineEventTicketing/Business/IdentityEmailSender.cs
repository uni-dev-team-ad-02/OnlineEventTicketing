using Microsoft.AspNetCore.Identity.UI.Services;
using OnlineEventTicketing.Helpers;

namespace OnlineEventTicketing.Business
{
    public class IdentityEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IConfiguration configuration, ILogger<IdentityEmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await EmailHelper.SendEmailAsync(email, subject, htmlMessage, _configuration, _logger);
        }
    }
}