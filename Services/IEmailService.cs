namespace Workflows.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendApprovalPendingNotificationAsync(string approverEmail, int requisitionId);
        Task SendApprovalMadeNotificationAsync(string requesterEmail, int requisitionId);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Implement email sending logic here
            // You can use a third-party email service or configure your own SMTP server
            var smtpServer = _configuration["SMTP:Server"];
            var smtpPort = int.Parse(_configuration["SMTP:Port"]);
            var smtpUsername = _configuration["SMTP:Username"];
            var smtpPassword = _configuration["SMTP:Password"];
            var smtpUseSsl = bool.Parse(_configuration["SMTP:UseSsl"]);
            var fromEmail = _configuration["SMTP:FromEmail"];

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = smtpUseSsl;

                var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                mailMessage.IsBodyHtml = true;

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendApprovalPendingNotificationAsync(string approverEmail, int requisitionId)
        {
            // Prepare the email subject and body for the approval pending notification
            string subject = "Approval Pending";
            string body = $"A new approval is pending for requisition {requisitionId}. Please review and take action.";

            await SendEmailAsync(approverEmail, subject, body);
        }

        public async Task SendApprovalMadeNotificationAsync(string requesterEmail, int requisitionId)
        {
            // Prepare the email subject and body for the approval made notification
            string subject = "Approval Made";
            string body = $"Your requisition {requisitionId} has been approved.";

            await SendEmailAsync(requesterEmail, subject, body);
        }
    }
}
