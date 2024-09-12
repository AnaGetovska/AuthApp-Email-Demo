using SendGrid;
using SendGrid.Helpers.Mail;

namespace Email2FAuth.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string otp, string username)
        {
            var subject = "Your 2FA Code";
            var sendGridKey = _configuration["SendGridKey"];
            var baseUrl = _configuration["ExternalUri"];
            var url = $"{baseUrl}verify?username={username}&otp={otp}";
            ArgumentNullException.ThrowIfNull(sendGridKey, nameof(sendGridKey));
            var message = $@"
            <p>Your one-time password (OTP) is: <strong>{otp}</strong></p>";

            //TODO: Redirect user with button
            //var message = $@"
            //<p>Your one-time password (OTP) is: <strong>{otp}</strong></p>
            //<p>Please click the button below to verify your login:</p>
            //<a href='{url}' style='background-color: #4CAF50; color: white;
            //padding: 10px 20px; text-align: center; text-decoration: none; display: inline-block;'>Verify Login</a>";

            await Execute(sendGridKey, subject, message, toEmail);
        }

        private async Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            var client = new SendGridClient(apiKey);

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_configuration["From"], _configuration["Name"]),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(toEmail));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            var response = await client.SendEmailAsync(msg);
            var responseBody = await response.DeserializeResponseBodyAsync();
            _logger.LogInformation(response.IsSuccessStatusCode
                ? $"Email to {toEmail} queued successfully!"
                : $"Failure Email to {toEmail}");
        }
    }
}
