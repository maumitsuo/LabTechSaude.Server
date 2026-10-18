using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Emails.Commands
{
    public class EnviarEmailCommand : ICommand
    {
        public IReadOnlyList<string> Destinatarios { get; }
        public string Assunto { get; }
        public string Corpo { get; }
        public bool CorpoEhHtml { get; }
        public IReadOnlyList<string> CopiaOculta { get; }
        public IReadOnlyList<EmailAnexo> Anexos { get; }
        public string? ResponderPara { get; }

        public EnviarEmailCommand(
            string destinatario,
            string assunto,
            string corpo,
            IEnumerable<string>? copiaOculta = null,
            IEnumerable<EmailAnexo>? anexos = null,
            string? responderPara = null,
            bool corpoEhHtml = false)
            : this(new[] { destinatario }, assunto, corpo, copiaOculta, anexos, responderPara, corpoEhHtml)
        {
        }

        public EnviarEmailCommand(
            IEnumerable<string> destinatarios,
            string assunto,
            string corpo,
            IEnumerable<string>? copiaOculta = null,
            IEnumerable<EmailAnexo>? anexos = null,
            string? responderPara = null,
            bool corpoEhHtml = false)
        {
            ArgumentNullException.ThrowIfNull(destinatarios);
            ArgumentNullException.ThrowIfNull(assunto);
            ArgumentNullException.ThrowIfNull(corpo);

            Destinatarios = destinatarios.ToArray();
            Assunto = assunto;
            Corpo = corpo;
            CorpoEhHtml = corpoEhHtml;
            CopiaOculta = copiaOculta?.ToArray() ?? Array.Empty<string>();
            Anexos = anexos?.ToArray() ?? Array.Empty<EmailAnexo>();
            ResponderPara = responderPara;
        }
    }

    public sealed record EmailAnexo(string NomeArquivo, byte[] Conteudo, string? TipoConteudo = null);
}
