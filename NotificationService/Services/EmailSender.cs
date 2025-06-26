using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string textBody, string htmlBody)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["Email:From"] ?? "noreply@banking-system.test"));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    TextBody = textBody,
                    HtmlBody = htmlBody
                };
                email.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _config["Email:SmtpServer"] ?? "sandbox.smtp.mailtrap.io",
                    int.Parse(_config["Email:Port"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                // Fix for GSSAPI/XOAUTH2 issue
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.AuthenticationMechanisms.Remove("GSSAPI");

                await client.AuthenticateAsync(
                    _config["Email:Username"],
                    _config["Email:Password"]
                );

                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                Console.WriteLine($"📧 [EmailSender] Successfully sent email to: {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [EmailSender] Failed to send email to: {to}");
                Console.WriteLine($"[Exception] {ex.Message}");
            }
        }
    }
}