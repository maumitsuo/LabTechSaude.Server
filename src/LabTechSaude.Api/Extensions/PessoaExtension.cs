using LabTechSaude.Api.ViewModels;
using LabTechSaude.Domain.Core.ValueObjects;
using LabTechSaude.Domain.Pessoas;

namespace LabTechSaude.Api.Extensions
{
    public static class PessoaExtension
    {
        public static PessoaViewModel? ToViewModel(this Pessoa pessoa)
        {
            if (pessoa == null) 
                return null;

            return new PessoaViewModel(
                pessoa.Id,
                pessoa.Nome,
                pessoa.Cpf.ToString()
            );
        }

        public static IEnumerable<PessoaViewModel> ToViewModel(this IEnumerable<Pessoa> pessoas)
        {
            if (!pessoas.Any())
                return new List<PessoaViewModel>();

            return pessoas

                .Select(p => p.ToViewModel()!)
                .ToList();
        }

        public static Pessoa? ToDomain(this PessoaViewModel pessoaViewModel)
        {
            if (pessoaViewModel == null)
                return null;

            return new Pessoa(
                pessoaViewModel.Id,
                pessoaViewModel.Nome,
                new Cpf(pessoaViewModel.Cpf)
            );
        }
    }
}
