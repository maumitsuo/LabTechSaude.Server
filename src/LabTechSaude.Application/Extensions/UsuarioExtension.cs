using LabTechSaude.Application.Services.Usuarios.Commands;
using LabTechSaude.Application.ViewModels.Usuarios;
using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Usuarios;

namespace LabTechSaude.Application.Extensions
{
    public static class UsuarioExtension
    {
        public static UsuarioViewModel? ToViewModel(this Usuario usuario)
        {
            if (usuario == null) 
                return null;

            return new UsuarioViewModel 
            { 
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email.ToString(),
                Cpf = usuario.Cpf.ToString()
            };
        }

        public static IEnumerable<UsuarioViewModel> ToViewModel(this IEnumerable<Usuario> usuarios)
        {
            if (!usuarios.Any())
                return new List<UsuarioViewModel>();

            return usuarios

                .Select(p => p.ToViewModel()!)
                .ToList();
        }

        public static CadastrarUsuarioCommand ToCadastrarCommand(this UsuarioViewModel usuarioViewModel)
        {
            return new CadastrarUsuarioCommand(
                usuarioViewModel.Nome,
                usuarioViewModel.Email,
                usuarioViewModel.Cpf
            );
        }

        public static AtualizarUsuarioCommand ToAtualizarCommand(this UsuarioViewModel usuarioViewModel)
        {
            return new AtualizarUsuarioCommand(
                usuarioViewModel.Id,
                usuarioViewModel.Nome
            );
        }
    }
}
