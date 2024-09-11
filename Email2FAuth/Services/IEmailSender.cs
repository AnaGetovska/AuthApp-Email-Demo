namespace Email2FAuth.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string otp, string username, string baseUrl);
    }
}
