using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class ConfirmarEmailExtension
    {
        public static ConfirmarCodigoEmailCommand ToCommand(this ConfirmarEmailViewModel confirmarEmailViewModel)
        {
            return new ConfirmarCodigoEmailCommand(confirmarEmailViewModel.Email, confirmarEmailViewModel.Code);
        }
    }
}
