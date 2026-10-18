using LabTechSaude.Application.Extensions;
using LabTechSaude.Application.ViewModels;
using LabTechSaude.Domain.Usuarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Services.Usuarios.Queries
{
    public class UsuarioQuery : IUsuarioQuery
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioQuery(
            IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<UsuarioViewModel?> ObterPorId(Guid id)
        {
            return (await _usuarioRepository.ObterPorId(id))?.ToViewModel();
        }

        public async Task<IEnumerable<UsuarioViewModel>> ObterTodos()
        {
            return (await _usuarioRepository.ObterTodos()).ToViewModel();
        }

    }
}
