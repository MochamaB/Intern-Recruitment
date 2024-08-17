using System.Net;
using System.Net.Mail;

namespace Workflows.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);

        Task SendRequistionCreatedNotificationAsync(string requesterEmail, int requisitionId);
        Task SendApprovalPendingNotificationAsync(string approverEmail, int requisitionId);
        Task SendApprovalMadeNotificationAsync(string requesterEmail,string currentApproverName, string currentStep, int requisitionId);
        Task SendApprovalRejectedNotificationAsync(string requesterEmail, int requisitionId);
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
            try
            {
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
            catch (Exception ex)
            {
                // Log the exception details if needed
                // Throw a custom exception with a user-friendly message
                throw new ApplicationException("There was a problem sending the email. Please try again later.", ex);
            }
        }


        public async Task SendRequistionCreatedNotificationAsync(string requesterEmail, int requisitionId)
        {
            // Prepare the email subject and body for the approval pending notification
            string subject = "New Intern Requisition Created";
            string link = $"https://localhost:7119/Requisitions/Details/{requisitionId}";
            string body = $@"
                            Dear Sir/Madam.<br><br>
                            A new intern requisition has been created with requisition Id {requisitionId}.<br>
                            The Application Has been passed over to your Supervisor/Hod for Approval.<br>
                            Kindly Await An email Notification on the Status of this application..<br><br>
                            <a href='{link}'>Click here to review the requisition</a><br><br>
                            -----------------------[ This is an Automated Email, Do not reply ] ---------------

                        ";

            await SendEmailAsync(requesterEmail, subject, body);
        }

        public async Task SendApprovalPendingNotificationAsync(string approverEmail, int requisitionId)
        {
            // Prepare the email subject and body for the approval pending notification
            string subject = "Intern Requisition Approval Pending";
            string link = $"https://localhost:7119/Requisitions/Details/{requisitionId}";
            string body = $@"
                            Dear Sir/Madam.<br><br>
                            An new approval worflow for the intern requisition no {requisitionId} has been assigned to you.<br>
                            Please review and take action.<br><br>
                            <a href='{link}'>Click here to review the requisition</a><br><br>
                            -----------------------[ This is an Automated Email, Do not reply ] ---------------

                        ";

            await SendEmailAsync(approverEmail, subject, body);
        }

        public async Task SendApprovalMadeNotificationAsync(string requesterEmail, string currentApproverName, string currentStep, int requisitionId)
        {
            // Prepare the email subject and body for the approval made notification
            string subject = "Intern Requisition Approved";
            string link = $"https://localhost:7119/Requisitions/Details/{requisitionId}";
            string body = $@"
                            Dear Sir/Madam.<br><br>
                            This is to notify you that the intern requisition id {requisitionId}.<br>
                            Approval step {currentStep} Has been Approved by {currentApproverName}.<br><br>
                            <a href='{link}'>Click here to review the requisition</a><br><br>
                            -----------------------[ This is an Automated Email, Do not reply ] ---------------

                        ";

            await SendEmailAsync(requesterEmail, subject, body);
        }

        public async Task SendApprovalRejectedNotificationAsync(string requesterEmail, int requisitionId)
        {
            // Prepare the email subject and body for the approval made notification
            string subject = "Intern Requisition Approval Rejected";
            string link = $"https://localhost:7119/Requisitions/Details/{requisitionId}";
            string body = $@"
                            Dear Sir/Madam.<br><br>
                            This is to notify you that the intern requisition id {requisitionId}.<br>
                            Has been rejected.<br><br>
                            <a href='{link}'>Click here to review the requisition</a><br><br>
                            -----------------------[ This is an Automated Email, Do NOT reply ] ---------------

                        ";

            await SendEmailAsync(requesterEmail, subject, body);
        }
    }
}
