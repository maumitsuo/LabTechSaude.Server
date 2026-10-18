using LabTechSaude.Api.Configs;
using LabTechSaude.Application.Commands;
using LabTechSaude.Application.Commands.Dispatchers;
using LabTechSaude.Application.Commands.Handlers;
using LabTechSaude.Application.Notifications;
using LabTechSaude.Domain.Core.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LabTechSaude.Api.Services.Emails.Commands
{
    public class EmailCommandHandler : BaseCommandHandler,
        ICommandHandler<EnviarEmailCommand, bool>
    {
        private readonly SmtpConfig _smtpConfig;

        public EmailCommandHandler(
            Notificador notificador, 
            IUnitOfWork unitOfWork, 
            ICommandDispatcher commandDispatcher,
            IOptions<SmtpConfig> smtpConfig) 
            : base(notificador, unitOfWork, commandDispatcher)
        {
            _smtpConfig = smtpConfig.Value;
        }

        public async Task<bool> Handle(EnviarEmailCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ValidarConfiguracao();

            if (command.Destinatarios.Count == 0)
                throw new ArgumentException("Informe ao menos um destinatário.", nameof(command));

            var mensagem = CriarMensagem(command);

            try
            {
                using var cliente = new SmtpClient();

                await cliente.ConnectAsync(
                    _smtpConfig.Host,
                    _smtpConfig.Port,
                    SecureSocketOptions.Auto,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(_smtpConfig.UserName) || !string.IsNullOrWhiteSpace(_smtpConfig.Password))
                    await cliente.AuthenticateAsync(_smtpConfig.UserName, _smtpConfig.Password, cancellationToken);

                await cliente.SendAsync(mensagem, cancellationToken);
                await cliente.DisconnectAsync(true, cancellationToken);

                return true;
            }
            catch (Exception)
            {
                return NotificarErro("Falha ao enviar o e-mail.");
            }
        }

        private MimeMessage CriarMensagem(EnviarEmailCommand command)
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(_smtpConfig.SenderName, _smtpConfig.SenderEmail));

            foreach (var destinatario in command.Destinatarios)
                mensagem.To.Add(MailboxAddress.Parse(destinatario));

            foreach (var email in command.CopiaOculta)
                mensagem.Bcc.Add(MailboxAddress.Parse(email));

            if (!string.IsNullOrWhiteSpace(command.ResponderPara))
                mensagem.ReplyTo.Add(MailboxAddress.Parse(command.ResponderPara));

            mensagem.Subject = command.Assunto;

            var corpo = new BodyBuilder();
            if (command.CorpoEhHtml)
                corpo.HtmlBody = command.Corpo;
            else
                corpo.TextBody = command.Corpo;

            foreach (var anexo in command.Anexos)
            {
                var tipoConteudo = string.IsNullOrWhiteSpace(anexo.TipoConteudo)
                    ? ContentType.Parse("application/octet-stream")
                    : ContentType.Parse(anexo.TipoConteudo);
                corpo.Attachments.Add(anexo.NomeArquivo, anexo.Conteudo, tipoConteudo);
            }

            mensagem.Body = corpo.ToMessageBody();
            return mensagem;
        }

        private void ValidarConfiguracao()
        {
            if (string.IsNullOrWhiteSpace(_smtpConfig.Host))
                throw new InvalidOperationException("Host não configurado na seção SmtpConfig.");

            if (_smtpConfig.Port is < 1 or > 65535)
                throw new InvalidOperationException("Port inválida na seção SmtpConfig.");

            if (string.IsNullOrWhiteSpace(_smtpConfig.SenderEmail))
                throw new InvalidOperationException("SenderEmail não configurado na seção SmtpConfig.");

            if (string.IsNullOrWhiteSpace(_smtpConfig.UserName) != string.IsNullOrWhiteSpace(_smtpConfig.Password))
                throw new InvalidOperationException("Configure UserName e Password juntos na seção SmtpConfig.");
        }
    }
}
