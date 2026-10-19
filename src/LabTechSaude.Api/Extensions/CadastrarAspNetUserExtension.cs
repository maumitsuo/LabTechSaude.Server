using LabTechSaude.Api.Services.Accounts.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class CadastrarAspNetUserExtension
    {
        public static CadastrarAspNetUserCommand ToCommand(this CadastrarAspNetUserViewModel cadastrarAspNetUserViewModel)
        {
            return new CadastrarAspNetUserCommand(cadastrarAspNetUserViewModel.Nome, cadastrarAspNetUserViewModel.Email,
                cadastrarAspNetUserViewModel.Cpf, cadastrarAspNetUserViewModel.Senha);
        }
    }
}
