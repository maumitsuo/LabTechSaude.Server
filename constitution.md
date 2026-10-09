# Constituição — LabTechSaude

Este documento define os princípios que toda especificação, plano e implementação do LabTechSaude deve respeitar. Ele orienta decisões de produto e engenharia; não substitui os requisitos detalhados de cada funcionalidade.

## Princípios

### 1. Especificar antes de implementar

- Toda funcionalidade ou mudança relevante começa por uma especificação revisável, escrita antes da implementação.
- A especificação descreve o problema e os usuários, escopo e exclusões, cenários, requisitos funcionais e não funcionais e critérios de aceitação observáveis.
- Quando aplicável, também registra mudanças de API, modelo de dados e migrações, impactos em dados pessoais, riscos, dependências e estratégia de testes.
- Planos e tarefas devem rastrear os requisitos e critérios de aceitação. Uma tarefa só está concluída quando seu resultado pode ser verificado.
- Correções pequenas e isoladas podem usar uma especificação enxuta, mas devem explicitar o comportamento esperado e como será validado.

### 2. Preservar os limites da arquitetura

- A solução mantém responsabilidades separadas entre `Api`, `Application`, `Domain` e `Data`.
- `Domain` concentra regras e conceitos de negócio e não depende das camadas externas.
- `Application` orquestra casos de uso e depende do domínio;
- `Data` implementa persistência e contratos definidos pelo domínio.
- `Api` expõe os contratos HTTP e compõe as dependências da aplicação. Regras de negócio não devem ser implementadas em controllers nem acopladas ao banco de dados.
- Novas dependências entre camadas devem apontar para dentro, em direção ao domínio. Qualquer exceção exige justificativa na especificação.

### 3. Proteger dados pessoais e de saúde

- Dados pessoais e de saúde são tratados como sensíveis: coletar somente o necessário, restringir o acesso e evitar exposição em logs, mensagens de erro, exemplos, documentação e dados de teste.
- Segredos e credenciais não devem ser armazenados no código ou em arquivos versionados de configuração.
- Requisitos de privacidade, retenção, exclusão e acesso devem ser considerados na especificação sempre que a mudança afetar dados pessoais.
- Testes e exemplos usam dados fictícios; nunca dependem de dados reais de pacientes.

### 4. Manter segurança e validação próximas da regra

- Entradas externas devem ser validadas antes de produzir efeitos persistentes.
- Regras e invariantes do negócio pertencem ao domínio ou ao caso de uso apropriado; validação de formato e protocolo também deve ocorrer na fronteira da API quando pertinente.
- Consultas e alterações de dados devem respeitar autorização, integridade referencial e tratamento explícito de erros.
- Falhas não podem ser apresentadas como sucesso nem descartadas silenciosamente.

### 5. Tratar IA como apoio, não como autoridade clínica

- Saídas de IA são informativas e sujeitas a incerteza; não devem ser apresentadas como diagnóstico, prescrição ou substituto de avaliação profissional.
- Funcionalidades com IA devem especificar fontes de dados, limitações, tratamento de erros e indisponibilidade, privacidade e como o usuário entende ou contesta o resultado.
- Decisões clínicas não podem ser tomadas automaticamente com base apenas em inferências do sistema.

### 6. Evoluir contratos e dados com segurança

- Contratos HTTP devem ser explícitos e documentados, preservando o versionamento já adotado pela API.
- Mudanças incompatíveis de API precisam de estratégia de versionamento e transição descrita na especificação.
- Mudanças de esquema persistente devem ser feitas por migrações versionadas; não se altera o banco de produção por procedimentos manuais não rastreáveis.
- Migrações devem considerar integridade dos dados existentes e estratégia de aplicação ou reversão.

### 7. Verificar comportamento com testes

- Critérios de aceitação devem ser cobertos por testes automatizados sempre que viável.
- Regras do domínio devem ter testes unitários; fluxos de aplicação devem cobrir resultados e falhas relevantes; mudanças de API ou persistência devem incluir testes de integração quando o risco justificar.
- Toda mudança deve passar por build e pelos testes afetados. A especificação deve registrar verificações manuais quando um comportamento não puder ser automatizado.
- Testes devem ser determinísticos e não depender de serviços externos ou dados pessoais reais.

### 8. Manter mudanças pequenas, legíveis e consistentes

- Implementar apenas o escopo aprovado; decisões adicionais devem voltar à especificação antes de ampliar a mudança.
- Reutilizar padrões e dependências existentes quando adequados, evitando abstrações ou infraestrutura sem necessidade demonstrada.
- Documentação, nomes e exemplos devem seguir o idioma e as convenções já usados no contexto alterado.
- Atualizar documentação e contratos afetados junto com a implementação.

## Fluxo de desenvolvimento orientado a especificações

Para cada mudança:

1. **Especificar:** registrar problema, usuários, escopo, requisitos, critérios de aceitação, riscos e exclusões.
2. **Planejar:** identificar camadas e contratos afetados, decisões técnicas, dados/migrações, testes e dependências.
3. **Decompor:** criar tarefas pequenas, rastreáveis aos requisitos e verificáveis de forma independente.
4. **Implementar:** seguir os limites arquiteturais e não ampliar o escopo sem atualizar a especificação.
5. **Validar:** executar build, testes e verificações definidas; comparar o resultado com cada critério de aceitação.
6. **Registrar:** atualizar documentação e anotar desvios, riscos residuais ou critérios não atendidos.

## Conformidade e alterações desta constituição

- Especificações, planos e revisões devem apontar quais princípios se aplicam e registrar qualquer exceção com motivo, impacto e mitigação.
- Uma exceção não pode ignorar privacidade, segurança ou integridade dos dados sem uma decisão explícita e documentada.
- Alterações nesta constituição devem explicar a motivação, o impacto nas especificações e no trabalho em andamento e a forma de adoção.
