using System;
using System.Collections.Generic;
using System.Text;

namespace LabTechSaude.Domain
{
    internal class Class1
    {
        public void Teste()
        {
            var pessoa = new Pessoas.Pessoa(Guid.NewGuid(), "João da Silva", new Core.ValueObjects.CPF.CPF("12345678909"));

            if (!pessoa.IsValid)
                foreach (var error in pessoa.Erros)
                    Console.WriteLine(error);


        }
    }
}
