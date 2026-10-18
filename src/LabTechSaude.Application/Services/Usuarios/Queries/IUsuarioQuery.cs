using LabTechSaude.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Application.Services.Usuarios.Queries
{
    public interface IUsuarioQuery
    {
        Task<UsuarioViewModel?> ObterPorId(Guid id);
        Task<IEnumerable<UsuarioViewModel>> ObterTodos();
        Task<UsuarioViewModel?> ObterPorEmail(string email);
    }
}
