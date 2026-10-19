using LabTechSaude.Api.Services.Accounts.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class EnviarCodigoConfirmarEmailExtension
    {
        public static EnviarConfirmarEmailCommand ToCommand(this EnviarConfirmarEmailViewModel enviarCodigoConfirmarEmailViewModel)
        {
            return new EnviarConfirmarEmailCommand(enviarCodigoConfirmarEmailViewModel.Email);
        }
    }
}
