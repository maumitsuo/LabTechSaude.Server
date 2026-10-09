using LabTechSaude.Application.ViewModels;
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

            return new UsuarioViewModel(
                usuario.Id,
                usuario.Nome,
                usuario.Cpf.ToString()
            );
        }

        public static IEnumerable<UsuarioViewModel> ToViewModel(this IEnumerable<Usuario> usuarios)
        {
            if (!usuarios.Any())
                return new List<UsuarioViewModel>();

            return usuarios

                .Select(p => p.ToViewModel()!)
                .ToList();
        }

        public static Usuario? ToDomain(this UsuarioViewModel usuarioViewModel)
        {
            if (usuarioViewModel == null)
                return null;

            return new Usuario(
                usuarioViewModel.Id,
                usuarioViewModel.Nome,
                new Cpf(usuarioViewModel.Cpf)
            );
        }
    }
}
