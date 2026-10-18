using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class ConfirmarEmailExtension
    {
        public static ConfirmarEmailCommand ToCommand(this ConfirmarEmailViewModel confirmarEmailViewModel)
        {
            return new ConfirmarEmailCommand(confirmarEmailViewModel.Email, confirmarEmailViewModel.Code);
        }
    }
}
