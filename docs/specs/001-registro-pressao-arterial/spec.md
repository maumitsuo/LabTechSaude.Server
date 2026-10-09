# Spec 001 — CRUD de pressão arterial

- **Status:** rascunho para revisão
- **Constituição aplicável:** `constitution.md`
- **Decisão de unicidade:** a combinação `UsuarioId` + `DataHora` é única. Cada campo pode se repetir em registros diferentes.

## Problema

Um usuário precisa manter um histórico de medições de pressão arterial associadas a um usuário cadastrado e a um instante específico. O sistema deve permitir registrar, consultar, editar e excluir medições, validar os valores informados e evitar registros duplicados para o mesmo usuário no mesmo instante.

## Objetivo

Implementar CRUD para medições com os campos:

`Id | UsuarioId | DataHora | Sistolica | Diastolica | Pulso | Observacoes`

Os limites definidos nesta spec são validações de plausibilidade dos dados de entrada. Não são faixas de normalidade, critérios diagnósticos nem recomendações clínicas.

## Usuário

Um usuário autorizado a gerenciar medições de um usuário cadastrado.

## Escopo

- Criar uma medição associada a um usuário existente.
- Consultar uma medição por `Id` e listar medições.
- Atualizar os dados de uma medição existente.
- Excluir uma medição existente.
- Validar os dados e preservar a unicidade ao criar ou atualizar.
- Preservar `Observacoes` como texto, sem gerar embeddings nesta entrega.

## Fora de escopo

- Gráficos, alertas, interpretação clínica ou diagnóstico.
- Geração, armazenamento, busca ou consulta de embeddings.
- Integração com aparelhos de medição ou serviços externos.
- Alterações no cadastro de usuários.
- Relatórios ou análises de tendências.

## Modelo de dados

| Campo | Tipo conceitual | Obrigatoriedade | Regra |
|---|---|---|---|
| `Id` | GUID | Gerado pelo sistema | Identificador imutável do registro |
| `UsuarioId` | GUID | Obrigatório | Deve referenciar um usuário existente |
| `DataHora` | Data/hora com offset | Obrigatório | Representa um instante inequívoco |
| `Sistolica` | Inteiro, mmHg | Obrigatório | De 50 a 250, inclusive |
| `Diastolica` | Inteiro, mmHg | Obrigatório | De 40 a 200, inclusive |
| `Pulso` | Inteiro, bpm | Obrigatório | De 50 a 250, inclusive |
| `Observacoes` | Texto | Opcional | Associado ao registro e preservado como texto |

O tamanho máximo de `Observacoes` deve ser definido no plano técnico considerando a persistência e o uso futuro do conteúdo para embeddings.

## Requisitos funcionais

### RF-1 — Criar medição

O sistema deve criar uma medição quando todos os dados obrigatórios forem válidos. `Id` é gerado pelo sistema e não pode ser definido pelo cliente.

### RF-2 — Consultar medição

O sistema deve permitir consultar uma medição por `Id` e `UsuarioId`. Se não existir, deve informar que o recurso não foi encontrado.

### RF-3 — Listar medições

O sistema deve permitir listar as medições existentes e filtrar por `UsuarioId`, para que o histórico de um usuário possa ser consultado sem misturá-lo com o de outros. Os detalhes de paginação e ordenação serão definidos no plano técnico.

### RF-4 — Atualizar medição

O sistema deve permitir atualizar `UsuarioId`, `DataHora`, `Sistolica`, `Diastolica`, `Pulso` e `Observacoes` de uma medição existente. `Id` permanece inalterado. Os novos dados devem satisfazer todas as regras desta spec, inclusive a associação a um usuário existente e a unicidade de `UsuarioId` + `DataHora`.

Se a medição não existir, o sistema não deve criar uma nova como efeito da atualização.

### RF-5 — Excluir medição

O sistema deve permitir excluir uma medição existente por `Id`. Se não existir, deve informar que o recurso não foi encontrado. A exclusão não deve remover nem alterar o usuário associado.

### RF-6 — Validar valores

Os limites são inclusivos:

| Campo | Unidade | Mínimo | Máximo |
|---|---:|---:|---:|
| `Sistolica` | mmHg | 50 | 250 |
| `Diastolica` | mmHg | 40 | 200 |
| `Pulso` | bpm | 50 | 250 |

Além dos limites individuais, `Sistolica` deve ser estritamente maior que `Diastolica`. Valores dentro dos limites não devem ser rejeitados por parecerem clinicamente preocupantes; o sistema não interpreta a medição.

### RF-7 — Associar a um usuário existente

`UsuarioId` deve referenciar um usuário cadastrado. Criação ou atualização não pode persistir uma medição para um usuário inexistente.

### RF-8 — Garantir unicidade por usuário e instante

Não pode haver mais de uma medição para a mesma combinação de `UsuarioId` e `DataHora`. Uma tentativa duplicada na criação ou atualização deve ser rejeitada sem alterar o registro que já ocupa essa combinação.

Dois registros com o mesmo `DataHora` para usuários diferentes são permitidos. Dois registros para o mesmo usuário em instantes diferentes são permitidos.

`DataHora` representa um instante inequívoco. A API deve receber data/hora em formato ISO 8601 com offset ou `Z`, e a comparação de unicidade deve considerar o instante normalizado em UTC. Assim, valores com offsets diferentes que representem o mesmo instante são duplicados para o mesmo usuário.

### RF-9 — Tratar observações como texto

`Observacoes` é opcional e, quando fornecida, deve ser preservada como texto vinculado à medição. Esta entrega não deve enviar o conteúdo a modelos de IA nem criar embeddings ou índices vetoriais. O formato deve permitir uma etapa futura de geração de embeddings sem alterar o significado do texto original.

## Requisitos não funcionais

- A persistência deve manter a integridade da associação com o usuário e da unicidade, inclusive sob requisições concorrentes; a regra não pode depender apenas de uma verificação prévia na aplicação.
- Dados de saúde e observações não devem ser incluídos em logs ou mensagens de erro.
- Respostas não devem expor medições de usuários fora do escopo solicitado.
- A implementação deve respeitar as camadas `Api`, `Application`, `Domain` e `Data` descritas na constituição.
- Operações de leitura e escrita devem ser assíncronas quando apropriado aos padrões da solução.

## Critérios de aceitação

1. **Criação válida:** dado um usuário existente e valores dentro dos limites, com sistólica maior que diastólica, ao criar a medição, o sistema persiste o registro, gera um `Id` e confirma a criação.
2. **Consulta por identificador:** dada uma medição existente, consultá-la por `Id` retorna os dados associados; um `Id` desconhecido resulta em recurso não encontrado.
3. **Listagem:** listar medições retorna os registros existentes; informar `UsuarioId` retorna somente medições daquele usuário.
4. **Atualização válida:** dada uma medição existente, enviar novos campos válidos atualiza essa medição sem alterar seu `Id`.
5. **Atualização inexistente:** tentar atualizar um `Id` desconhecido não cria uma medição.
6. **Exclusão:** excluir uma medição existente remove o registro; o usuário associado permanece cadastrado. Excluir um `Id` desconhecido resulta em recurso não encontrado.
7. **Limites inclusivos:** valores exatamente iguais aos mínimos e máximos definidos são aceitos quando todas as demais regras forem satisfeitas.
8. **Valores fora dos limites:** criação ou atualização com qualquer valor abaixo do mínimo ou acima do máximo é rejeitada, sem persistência ou alteração do registro.
9. **Relação entre pressões:** criação ou atualização com `Sistolica` igual ou menor que `Diastolica` é rejeitada.
10. **Usuário inexistente:** criação ou atualização com `UsuarioId` que não identifica um usuário cadastrado é rejeitada.
11. **Duplicidade:** criar ou atualizar uma medição para uma combinação de usuário e instante já utilizada por outro registro é rejeitado; registros existentes não são sobrescritos.
12. **Instantes equivalentes:** offsets diferentes que representam o mesmo instante para o mesmo usuário são tratados como duplicidade.
13. **Mesmo instante, usuários diferentes:** o sistema permite medições no mesmo instante para usuários diferentes.
14. **Observações opcionais:** é possível criar ou atualizar uma medição sem observações; quando fornecidas, elas permanecem associadas ao registro e não são enviadas a um serviço de IA nesta entrega.
15. **Concorrência:** requisições simultâneas para criar ou atualizar medições com a mesma combinação de usuário e instante não podem resultar em duplicidade persistida.

## Contrato de erro esperado

- Dados ausentes ou inválidos: resposta de validação identificando os campos inválidos.
- `UsuarioId` inexistente: resposta informando que o usuário não foi encontrado.
- Medição inexistente em consulta, atualização ou exclusão: resposta de recurso não encontrado.
- Combinação duplicada de usuário e instante: conflito de duplicidade, sem sobrescrita.
- Falha de persistência: reportar falha; não retornar uma resposta de sucesso.

Os códigos HTTP exatos e os formatos dos corpos devem seguir os padrões existentes da API e ser definidos no plano técnico, mantendo a distinção entre validação, recurso inexistente e conflito.

## Diretrizes para testes

- Testar cada limite mínimo e máximo, valores fora dos limites e sistólica igual ou menor que diastólica.
- Testar CRUD completo: criação, consulta por `Id`, listagem sem filtro e por `UsuarioId`, atualização e exclusão.
- Testar usuário inexistente, registro inexistente e duplicidade em criação e atualização.
- Testar offsets equivalentes e instante repetido para usuários diferentes.
- Testar a restrição de unicidade na persistência, incluindo requisições concorrentes.
- Testar preservação de observações sem enviar conteúdo a integrações externas.
- Usar somente dados fictícios.

## Decisões para o plano técnico

- Definir o nome final da entidade, tabela, rotas, DTOs e formato das respostas, seguindo os padrões existentes.
- Definir paginação, ordenação e códigos HTTP da listagem e das demais operações.
- Definir o tamanho máximo de `Observacoes` e o tratamento de texto vazio, considerando o uso futuro em embeddings.
- Definir o comportamento de exclusão de um usuário que já possua medições, preservando a integridade referencial.
- Confirmar a estratégia de precisão do timestamp no PostgreSQL para que a normalização e a unicidade representem o mesmo instante.
