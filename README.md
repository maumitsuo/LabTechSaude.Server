# 🧪 LabTechSaude

O **LabTechSaude** é um projeto desenvolvido como um laboratório prático para explorar a integração de **Inteligência Artificial na Engenharia de Software**, abordando a criação de especificações técnicas, definição de *skills* de IA, arquitetura **RAG (Retrieval-Augmented Generation)** e padrões de projeto em **.NET**.

O domínio da aplicação é voltado para o acompanhamento de saúde e investigação de hábitos, permitindo registrar o histórico alimentar, leituras de pressão arterial e ocorrências de reações alérgicas para identificação de padrões por meio de IA.

---

## 🎯 Objetivos de Estudo

- [ ] **Engenharia de IA & Skills:** Mapear e estruturar diretrizes e *skills* bem definidas para orientar assistentes de IA na especificação e desenvolvimento do projeto.
- [ ] **Padrões de Arquitetura:** Implementar boas práticas de engenharia de software e Clean Code.
- [ ] **Mecanismo de RAG:** Utilizar busca vetorial e IA para cruzar dados do diário de consumo (alimentos e bebidas) com episódios de reações alérgicas e variações de pressão.

---

## 🩺 Funcionalidades da Aplicação

1. **Gestão de Usuários:** Cadastro de perfis para acompanhamento de saúde.
2. **Diário de Consumo:** Registro diário de refeições, bebidas e horários.
3. **Sinais Vitais:** Registro e monitoramento de leituras de pressão arterial.
4. **Rastreamento Alérgico:** Registro de sintomas para análise e correlação via IA.

---

## 🛠️ Visão Geral das Tecnologias

- **Backend:** C# / .NET
- **Inteligência Artificial:** RAG, Embeddings e LLMs
- **Infraestrutura:** Docker
- **Documentação:** Especificações e Skills em Markdown

---

## 📁 Estrutura do Repositório

```text
LabTechSaude.Server/
├── docs/                            # Especificações técnicas e arquiteturais
├── .ai/                             # Prompts padronizados e Skills para a IA
├── src/
│   ├── LabTechSaude.Api/            # API RESTful em .NET (Endpoints, Controllers e Middlewares)
|   ├── LabTechSaude.Application/    # Casos de Uso
│   ├── LabTechSaude.Data/           # Infraestrutura de dados em .NET (EF Core, Migrations e PgVector)
│   └── LabTechSaude.Domain/         # Coração da aplicação (Entidades, Regras de Negócio e Interfaces)
├── docker-compose.yml               # Orquestração de containers (API, PostgreSQL/PgVector)
└── README.md