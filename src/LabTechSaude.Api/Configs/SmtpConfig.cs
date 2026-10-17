using MailKit.Security;

namespace LabTechSaude.Api.Configs
{
    public class SmtpConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.StartTls;
    }
}
