using LabTechSaude.Api.Services.Authentications.Commands;
using LabTechSaude.Api.ViewModels;

namespace LabTechSaude.Api.Extensions
{
    public static class EsqueciMinhaSenhaExtension
    {
        public static EsqueciMinhaSenhaCommand ToCommand(this EsqueciMinhaSenhaViewModel esqueciMinhaSenhaViewModel)
        {
            return new EsqueciMinhaSenhaCommand(esqueciMinhaSenhaViewModel.Email);
        }
    }
}
