# Instruções para o repositório

## Compilação, execução e testes

- Restaure as dependências: `dotnet restore LabTechSaude.Server.slnx`
- Compile a solução: `dotnet build LabTechSaude.Server.slnx`
- Execute a API: `dotnet run --project src\LabTechSaude.Api\LabTechSaude.Api.csproj`
- Configure a conexão com o banco de dados em `ConnectionStrings:DefaultConnection` (por exemplo, usando a variável de ambiente `ConnectionStrings__DefaultConnection`). Não inclua credenciais reais no controle de versão.
- Crie uma migração do EF Core com `dotnet ef migrations add <Name> --project src\LabTechSaude.Data\LabTechSaude.Data.csproj --startup-project src\LabTechSaude.Api\LabTechSaude.Api.csproj`. Aplique migrações com `dotnet ef database update --project src\LabTechSaude.Data\LabTechSaude.Data.csproj --startup-project src\LabTechSaude.Api\LabTechSaude.Api.csproj`.
- Atualmente, a solução não contém projetos de teste; `dotnet test LabTechSaude.Server.slnx` não encontra testes para executar. Quando um projeto de teste for adicionado, execute todos os testes com `dotnet test <path-to-test-project.csproj>` e um teste específico com `dotnet test <path-to-test-project.csproj> --filter "FullyQualifiedName~Namespace.ClassName.TestName"`. O repositório não tem um comando de lint configurado.

## Arquitetura

- `LabTechSaude.Domain` contém entidades, objetos de valor, regras do FluentValidation e contratos de persistência. Essa camada não deve depender das camadas externas.
- `LabTechSaude.Application` implementa casos de uso e lógica de consulta. Os comandos são encaminhados por `ICommandDispatcher` para implementações de `ICommandHandler<TCommand>`; os handlers validam entidades de domínio, usam interfaces de repositório e unidade de trabalho definidas no domínio e confirmam as alterações. Consultas e comandos são separados.
- `LabTechSaude.Data` implementa os contratos de persistência do domínio com EF Core e PostgreSQL. Mantenha o mapeamento de entidades para tabelas nas classes de mapeamento e as alterações de esquema do banco em migrações versionadas neste projeto.
- `LabTechSaude.Api` contém os controllers HTTP, o versionamento da API, a configuração e o registro de dependências. `Program.cs` integra as configurações do banco de dados, da API, da injeção de dependências e do OpenAPI. Não coloque regras de negócio nos controllers e registre novos serviços de aplicação ou dados em `DependencyInjectionConfig`.
- Em geral, uma requisição de gravação percorre um controller versionado, uma extensão de mapeamento de view model, o dispatcher de comandos, um handler da aplicação, a validação de domínio, o repositório e a unidade de trabalho. `MainController` converte as notificações dos handlers em respostas de validação.
- A solução e os projetos têm como alvo o .NET 10. O PostgreSQL é configurado em `DatabaseConfig`; os endpoints do OpenAPI e do Scalar ficam habilitados no ambiente de desenvolvimento.

## Convenções do repositório

- Preserve os limites entre as camadas: os contratos pertencem a Domain, a orquestração de casos de uso a Application, as implementações de infraestrutura a Data e a composição HTTP a Api.
- Siga o vocabulário de domínio e os nomes da API pública já existentes em português (`Usuario`, `Cpf`, nomes de operações e mensagens de validação em português). Mantenha os namespaces alinhados ao projeto e à camada.
- Defina invariantes de entidades e regras de negócio em Domain, seguindo o padrão existente de validação de entidades com FluentValidation. Os handlers de Application comunicam erros de validação e de operação por meio do `Notificador` com escopo; preserve esse fluxo de resposta para falhas em comandos.
- Defina as interfaces de repositório em Domain e implemente-as com EF Core em Data. Use os mapeamentos de `LabTechSaudeDbContext`; não configure o esquema em controllers ou handlers.
- Mantenha leituras e gravações assíncronas quando os contratos ao redor também forem assíncronos. Use as abstrações existentes de repositório e `IUnitOfWork` para persistência.
- Mantenha o versionamento das rotas da API no padrão estabelecido `api/v{version:apiVersion}/...` e mapeie view models de transporte para comandos usando métodos de extensão em Application.
- Siga `constitution.md` ao implementar funcionalidades: respeite os limites das camadas, considere a sensibilidade dos dados pessoais e de saúde, use migrações versionadas para alterações no esquema e mantenha as respostas de IA como informações, não como autoridade clínica. As especificações de funcionalidades ficam em `docs/specs/`.
- Nenhum método gerado deve ter mais de um `return`: use uma variável local de resultado, estruturas `if/else` aninhadas ou métodos auxiliares e retorne uma única vez, no final do método.
