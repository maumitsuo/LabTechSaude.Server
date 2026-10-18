using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class EfetuarLoginExtension
    {
        public static EfetuarLoginCommand ToCommand(this EfetuarLoginViewModel viewModel)
        {
            return new EfetuarLoginCommand(viewModel.Email, viewModel.Senha);
        }
    }
}
