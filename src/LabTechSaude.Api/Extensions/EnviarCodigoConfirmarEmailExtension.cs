using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class EnviarCodigoConfirmarEmailExtension
    {
        public static EnviarCodigoConfirmarEmailCommand ToCommand(this EnviarCodigoConfirmarEmailViewModel enviarCodigoConfirmarEmailViewModel)
        {
            return new EnviarCodigoConfirmarEmailCommand(enviarCodigoConfirmarEmailViewModel.Email);
        }
    }
}
