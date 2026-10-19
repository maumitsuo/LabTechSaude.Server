using LabTechSaude.Api.Services.Accounts.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class RedefinirSenhaExtension
    {
        public static RedefinirSenhaCommand ToCommand(        this RedefinirSenhaViewModel redefinirSenhaViewModel)
        {
                    return new RedefinirSenhaCommand(redefinirSenhaViewModel.Email, redefinirSenhaViewModel.Code, redefinirSenhaViewModel.Senha);
        }
    }
}
