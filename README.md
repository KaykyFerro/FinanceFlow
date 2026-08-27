# FinanceFlow

Sistema de gestão financeira pessoal desenvolvido para centralizar **receitas, despesas, contas, categorias, orçamentos e análises financeiras** em uma única aplicação web, com backend em .NET e PostgreSQL e uma versão Android baseada em WebView.

> **Status:** 🚧 Em desenvolvimento ativo. O núcleo financeiro já está funcional, com Dashboard, Transações, Relatórios, Categorias, Contas, Orçamentos, regras para investimentos, tema claro/escuro, identidade visual e build Android automatizado.
>
> **Observação:** a autenticação e o gerenciamento de sessão existem no projeto, mas ficam **fora da avaliação de funcionalidades abaixo**, conforme o roadmap atual.

---

## 🌐 Links

- **Aplicação web:** https://kaykyferro.github.io/FinanceFlow/
- **Autenticação:** https://kaykyferro.github.io/FinanceFlow/frontend/auth.html
- **Repositório:** https://github.com/KaykyFerro/FinanceFlow
- **Builds Android:** https://github.com/KaykyFerro/FinanceFlow/actions/workflows/android.yml

---

## 🎯 Objetivo do projeto

O FinanceFlow busca oferecer uma visão clara da vida financeira do usuário, permitindo:

- registrar entradas e despesas;
- organizar movimentações por conta e categoria;
- acompanhar o saldo das contas;
- visualizar o fluxo financeiro por período;
- acompanhar limites de gastos por categoria;
- separar investimentos do dinheiro operacional;
- consultar histórico completo de movimentações;
- analisar a evolução financeira através de relatórios;
- utilizar a aplicação tanto no navegador quanto em Android.

A aplicação foi estruturada para evoluir de uma aplicação web para uma solução financeira mais completa, mantendo frontend, API e persistência separados.

---

# ✅ O que já está implementado

## 📊 Dashboard

O Dashboard é a visão principal da aplicação e atualmente possui:

- seleção e navegação entre meses;
- total de entradas do mês;
- total de saídas do mês;
- patrimônio operacional;
- total investido;
- fluxo financeiro;
- listagem de contas e saldos;
- histórico/resumo de movimentações;
- visualização diária de entradas e saídas;
- acesso rápido para criar uma nova transação;
- estados vazios quando não existem dados no período.

### Regra importante de patrimônio

O backend diferencia dinheiro operacional de investimentos. Contas classificadas como investimento e transações relacionadas a investimentos não são tratadas como entradas ou despesas normais do fluxo de caixa. O saldo dos investimentos é apresentado separadamente.

Essa regra evita contabilização duplicada do dinheiro investido. fileciteturn86file0L2-L2

---

## 💸 Transações

O módulo de transações funciona como o histórico financeiro geral.

### Implementado

- criação de transações;
- edição de transações;
- exclusão de transações;
- entradas;
- despesas;
- associação com conta;
- associação opcional com categoria;
- descrição;
- valor;
- data;
- observações;
- confirmação da movimentação;
- listagem de todas as transações;
- filtros por data inicial;
- filtros por data final;
- filtro por categoria;
- filtro por tipo;
- limpeza dos filtros;
- identificação visual de entradas e despesas;
- quantidade de movimentações cadastradas.

As transações possuem `UserId`, `AccountId` e `CategoryId`, e o backend valida que conta e categoria pertencem ao usuário autenticado antes de criar ou alterar uma movimentação. fileciteturn86file0L2-L2

---

## 🏦 Contas

O domínio já possui suporte para contas financeiras.

### Tipos existentes

- Conta corrente;
- Poupança;
- Carteira;
- Dinheiro;
- Investimento.

Cada conta possui:

- instituição;
- nome;
- tipo;
- saldo;
- usuário proprietário;
- estrutura para configuração de rendimento.

O backend possui operações para:

- criar conta;
- editar conta;
- excluir conta;
- consultar contas no resumo financeiro.

Uma conta que possui transações não pode ser excluída diretamente, evitando perda de integridade do histórico. fileciteturn89file0L2-L2 fileciteturn86file0L2-L2

---

## 🏷️ Categorias

O sistema possui categorias associadas ao usuário.

### Implementado

- categorias de entrada;
- categorias de saída;
- nome da categoria;
- cor da categoria;
- criação de categorias;
- exclusão de categorias;
- associação de categorias às transações;
- categorias padrão criadas automaticamente quando necessário.

Existe proteção contra exclusão de uma categoria que já possui transações. fileciteturn86file0L2-L2

### Categorias padrão atuais

- Salário
- Freelance
- Alimentação
- Moradia
- Transporte
- Faculdade
- Lazer
- Saúde
- Assinaturas
- Outros

---

## 💰 Orçamentos

O backend já possui a entidade e operações básicas de orçamento por categoria e mês.

### Implementado

- orçamento associado a uma categoria;
- limite mensal;
- mês de referência;
- cálculo do valor gasto na categoria;
- criação/atualização de orçamento;
- exclusão de orçamento;
- restrição para evitar dois orçamentos da mesma categoria no mesmo mês.

O valor gasto é calculado a partir das despesas regulares da categoria no período, sem incluir transações tratadas como investimento. fileciteturn86file0L2-L2

---

## 📈 Relatórios

A aplicação possui uma área de relatórios com análise por período.

### Implementado

- seleção de mês;
- navegação entre meses;
- total de entradas;
- total de despesas;
- saldo do período;
- gastos por categoria;
- fluxo diário;
- estados vazios para períodos sem movimentações.

Os dados dos relatórios são derivados das movimentações armazenadas no backend.

---

## 📊 Fluxo financeiro

O FinanceFlow já possui estrutura para analisar o comportamento financeiro ao longo do mês.

O resumo financeiro fornece dados de:

```text
Entradas
   ↓
Despesas
   ↓
Saldo operacional

Investimentos
   ↓
Exibidos separadamente
```

O backend também disponibiliza os valores agrupados por dia para alimentar as visualizações de fluxo. fileciteturn86file0L2-L2

---

## 📊 Investimentos

O projeto já possui a **regra financeira de separação dos investimentos**.

### Implementado

- tipo de conta `Investment`;
- identificação de contas de investimento;
- identificação da categoria `Investimentos`;
- separação das transações de investimento do fluxo operacional;
- indicador separado para total investido;
- prevenção de dupla contagem no saldo das contas de investimento;
- estrutura de rendimento na entidade de conta.

A entidade de contas já prevê tipos de rendimento como poupança, percentual do CDI, taxa anual fixa e IPCA+. fileciteturn89file0L2-L2

> **Importante:** isso não significa que exista ainda um módulo completo de investimentos. A regra de separação está implementada, mas funcionalidades como rentabilidade histórica, aportes, resgates e carteira de ativos ainda precisam ser desenvolvidas.

---

## 🎨 Interface e identidade visual

A interface web possui identidade visual própria do FinanceFlow.

### Implementado

- logo oficial;
- nome FinanceFlow;
- menu lateral;
- Dashboard;
- Transações;
- Relatórios;
- Categorias;
- Orçamentos;
- Configurações;
- cards financeiros;
- tabelas;
- filtros;
- modais;
- mensagens de estado;
- notificações/toasts;
- layout responsivo;
- barra de navegação mobile;
- tema claro;
- tema escuro;
- detecção da preferência do sistema;
- persistência da preferência de tema no navegador.

A logo está disponível em `frontend/assets/financeflow-logo.svg`.

---

## 🌙 Tema

A aplicação possui três estados de preferência:

- `system`;
- `light`;
- `dark`.

Quando configurado como `system`, o FinanceFlow utiliza a preferência de tema do navegador/sistema operacional.

A preferência fica armazenada no navegador e o tema é aplicado também fora do Dashboard.

---

# 📱 Android

O projeto possui uma versão Android baseada em **WebView**.

### Configuração atual

- Nome: `FinanceFlow`;
- Package: `com.kaykyferro.financeflow`;
- WebView carregando a aplicação web publicada;
- JavaScript habilitado;
- DOM Storage habilitado;
- suporte à navegação de retorno;
- acesso HTTPS;
- build Release automatizado;
- GitHub Actions para geração do APK;
- artifact `FinanceFlow-APK`.

Documentação complementar:

`docs/android-webview.md`

O Android atual é um wrapper WebView. Ele ainda não é uma aplicação mobile nativa independente.

---

## 📥 APK

O APK é gerado através do GitHub Actions.

Fluxo atual:

```text
GitHub
  ↓
Checkout
  ↓
Java + Android SDK
  ↓
Projeto Android WebView
  ↓
Gradle Release
  ↓
FinanceFlow.apk
  ↓
Artifact FinanceFlow-APK
```

O workflow está em:

`.github/workflows/android.yml`

Também existem workflows auxiliares relacionados à construção do Android/WebView e manutenção automatizada do projeto.

> Para distribuição permanente, o APK ainda deve ser publicado em uma Release do GitHub ou posteriormente em uma loja de aplicativos.

---

# 🏗️ Arquitetura

O projeto utiliza uma separação entre API, domínio e infraestrutura.

```text
FinanceFlow/
├── database/
│   └── schema.sql
├── docs/
│   ├── architecture.md
│   └── android-webview.md
├── frontend/
│   ├── assets/
│   │   └── financeflow-logo.svg
│   ├── auth.html
│   └── reset-password.html
├── src/
│   ├── FinanceFlow.Api/
│   ├── FinanceFlow.Domain/
│   └── FinanceFlow.Infrastructure/
├── .github/
│   └── workflows/
├── Dockerfile
├── docker-compose.yml
├── index.html
└── README.md
```

---

## ⚙️ Backend

Tecnologias principais:

- C#;
- .NET 10;
- ASP.NET Core Web API;
- Entity Framework Core;
- PostgreSQL;
- Npgsql;
- JWT Bearer;
- Docker.

O `FinanceFlowDbContext` possui atualmente conjuntos para:

- Users;
- Accounts;
- Transactions;
- Categories;
- Budgets;
- RefreshTokens;
- AuthTokens.

As entidades financeiras principais já estão modeladas no domínio. fileciteturn92file0L2-L2

---

# 🗄️ Banco de dados

O ambiente local utiliza PostgreSQL através do Docker Compose.

Configuração de desenvolvimento:

```yaml
POSTGRES_DB: financeflow
POSTGRES_USER: financeflow
POSTGRES_PASSWORD: financeflow_dev
ports:
  - "5432:5432"
```

Para iniciar:

```bash
docker compose up -d
```

Para parar:

```bash
docker compose down
```

O projeto possui volume persistente para o PostgreSQL.

O domínio financeiro atualmente trabalha principalmente com:

```text
Users
Accounts
Transactions
Categories
Budgets
```

Além das estruturas relacionadas à autenticação.

---

# 🐳 Docker

A API possui Dockerfile multi-stage para .NET.

Fluxo:

```text
Restore
  ↓
Build
  ↓
Publish Release
  ↓
Runtime ASP.NET
```

O projeto também possui `docker-compose.yml` para o ambiente local.

---

# 🔌 API financeira atual

A API financeira está concentrada em:

`/api/finance`

Principais operações existentes:

| Recurso | Criar | Editar | Excluir | Resumo/consulta |
|---|:---:|:---:|:---:|:---:|
| Transações | ✅ | ✅ | ✅ | ✅ |
| Contas | ✅ | ✅ | ✅ | ✅ |
| Categorias | ✅ | ❌ | ✅ | ✅ |
| Orçamentos | ✅ | 🔄* | ✅ | ✅ |

`*` O endpoint de criação de orçamento atualiza o registro existente quando já existe um orçamento para a mesma categoria e mês.

O endpoint de resumo também entrega categorias, contas, transações, orçamentos e dados diários para o frontend. fileciteturn86file0L2-L2

---

# 📌 Estado atual dos módulos

| Módulo | Estado atual | Observação |
|---|---|---|
| Dashboard | 🟢 Implementado | Indicadores e fluxo financeiro |
| Transações | 🟢 Implementado | CRUD + filtros |
| Contas | 🟢 Backend implementado | CRUD de contas |
| Categorias | 🟢 Backend implementado | Criar/excluir |
| Relatórios | 🟢 Implementado | Análise mensal |
| Orçamentos | 🟢 Backend implementado | Limite por categoria/mês |
| Investimentos | 🟡 Parcial | Regra de separação implementada |
| Tema claro/escuro | 🟢 Implementado | Sistema + preferência local |
| Interface responsiva | 🟢 Implementado | Desktop + mobile web |
| Android WebView | 🟢 Implementado | Build automatizado |
| Autenticação | 🟡 Fora deste roadmap | Não considerada nesta avaliação |

---

# 🚧 O que ainda falta

Considerando **somente o produto financeiro** e ignorando autenticação, estes são os principais pontos que ainda faltam para transformar o FinanceFlow em uma solução mais completa.

## 🔴 Prioridade alta

### 1. Cartões de crédito

Ainda falta um módulo específico para:

- cadastro de cartões;
- limite total;
- limite disponível;
- fechamento da fatura;
- vencimento;
- fatura atual;
- faturas futuras;
- compras parceladas;
- parcelas individuais;
- pagamento da fatura;
- histórico de faturas;
- impacto da fatura no fluxo de caixa.

Esse é provavelmente o maior módulo financeiro que falta atualmente.

### 2. Transações recorrentes

Implementar:

- mensalidades;
- salários recorrentes;
- assinaturas;
- aluguel;
- contas recorrentes;
- periodicidade configurável;
- data inicial/final;
- geração automática das ocorrências;
- opção de editar somente uma ocorrência ou toda a série.

### 3. Metas financeiras

Criar um módulo para objetivos como:

- reserva de emergência;
- comprar carro;
- viagem;
- entrada de imóvel;
- objetivo de investimento.

Deve permitir:

- valor-alvo;
- valor atual;
- prazo;
- progresso;
- aportes;
- percentual concluído.

### 4. Calendário financeiro

Adicionar uma visualização mensal mostrando:

- entradas previstas;
- despesas previstas;
- contas recorrentes;
- vencimentos;
- parcelas;
- faturas;
- eventos financeiros.

Isso será especialmente importante depois da implementação de cartões e recorrências.

---

## 🟠 Prioridade média

### 5. Evoluir o módulo de investimentos

A regra básica já existe, mas falta transformar isso em um módulo completo.

Sugestão:

- carteira de investimentos;
- ativos;
- quantidade;
- preço médio;
- preço atual;
- aportes;
- resgates;
- rentabilidade;
- rentabilidade percentual;
- dividendos;
- juros;
- evolução patrimonial;
- histórico de patrimônio;
- distribuição por classe de ativo.

### 6. Histórico patrimonial

Hoje existe o patrimônio operacional do período, mas ainda falta uma série histórica para responder perguntas como:

> "Quanto eu tinha há 6 meses?"

> "Meu patrimônio está crescendo?"

> "Quanto cresci desde janeiro?"

Seria interessante armazenar snapshots mensais ou calcular a evolução de forma consistente a partir do histórico financeiro.

### 7. Melhorar categorias

O módulo atual cobre criação e exclusão, mas pode evoluir com:

- edição de categoria;
- alteração de cor;
- ícones;
- subcategorias;
- categorias personalizadas mais completas;
- ordenação;
- arquivamento em vez de exclusão.

### 8. Melhorar contas

O backend já possui CRUD, mas o módulo pode evoluir com:

- saldo inicial separado das movimentações;
- conciliação bancária;
- transferência entre contas;
- contas conjuntas;
- arquivamento;
- histórico de saldo;
- configuração completa de rendimento;
- indicadores por instituição.

### 9. Transferências entre contas

Atualmente o modelo de transação representa entrada ou despesa. Ainda falta uma operação financeira específica de:

```text
Conta A
   ↓ transferência
Conta B
```

Sem que isso seja interpretado como renda ou despesa.

Esse recurso é **muito importante** para evitar distorções nos relatórios.

---

## 🟡 Prioridade futura

### 10. Relatórios avançados

Evoluir os relatórios com:

- comparação mês a mês;
- comparação anual;
- evolução de despesas;
- evolução de receitas;
- taxa de economia;
- maiores categorias;
- maiores despesas;
- evolução patrimonial;
- filtros personalizados;
- exportação CSV;
- exportação PDF;
- gráficos mais avançados.

### 11. Importação de extratos

Permitir importar movimentações bancárias através de arquivos como:

- CSV;
- OFX;
- eventualmente outros formatos bancários.

O sistema poderia:

```text
Arquivo bancário
      ↓
Leitura
      ↓
Identificação de movimentações
      ↓
Sugestão de categorias
      ↓
Confirmação
      ↓
FinanceFlow
```

### 12. Busca global

Adicionar pesquisa por:

- descrição;
- categoria;
- conta;
- valor;
- data;
- observação.

### 13. Notificações e alertas

Exemplos:

- orçamento próximo do limite;
- orçamento ultrapassado;
- conta vencendo;
- fatura próxima do vencimento;
- meta atrasada;
- saldo abaixo de determinado valor.

### 14. Experiência mobile nativa

O Android atual funciona como WebView. O próximo nível seria uma aplicação mobile realmente nativa ou multiplataforma, com:

- navegação mobile própria;
- armazenamento local/cache;
- melhor experiência offline;
- notificações;
- biometria;
- integração mais profunda com o sistema operacional.

---

# 🧠 Roadmap recomendado

Se a intenção for terminar o **núcleo financeiro** antes de adicionar recursos secundários, a ordem que faz mais sentido é:

```text
1. Transferências entre contas
        ↓
2. Cartões de crédito + faturas
        ↓
3. Transações recorrentes
        ↓
4. Calendário financeiro
        ↓
5. Metas financeiras
        ↓
6. Investimentos completos
        ↓
7. Histórico patrimonial
        ↓
8. Relatórios avançados
        ↓
9. Importação de extratos
        ↓
10. Notificações e automações
        ↓
11. Evolução do Android
```

A prioridade número **1** merece atenção especial: sem transferências explícitas, movimentações entre contas podem acabar aparecendo como receita/despesa e distorcer os indicadores.

---

# 🔒 Segurança e produção

O projeto possui estrutura de autenticação, tokens e separação de usuário no backend. A autenticação não faz parte do roadmap funcional deste documento.

Para produção, ainda é necessário revisar cuidadosamente:

- secrets;
- variáveis de ambiente;
- CORS;
- JWT;
- credenciais do banco;
- HTTPS;
- políticas de acesso;
- logs;
- backups;
- migrations;
- proteção contra abuso da API.

---

# 🧪 Testes

O projeto ainda precisa evoluir sua cobertura automatizada.

Recomendação:

### Backend

- testes unitários de domínio;
- testes dos controllers;
- testes de regras financeiras;
- testes de autorização por usuário;
- testes de integração com PostgreSQL.

### Frontend

- testes dos filtros;
- testes de cálculo dos indicadores;
- testes das telas principais;
- testes de criação/edição/exclusão;
- testes responsivos.

### Regras financeiras críticas

Testar principalmente:

- entradas;
- despesas;
- investimentos;
- transferências;
- orçamentos;
- parcelamentos;
- recorrências;
- saldo por conta;
- patrimônio.

---

# 🛠️ Stack atual

| Tecnologia | Uso |
|---|---|
| C# | Backend |
| .NET 10 | Runtime/API |
| ASP.NET Core | Web API |
| Entity Framework Core | ORM |
| PostgreSQL | Banco de dados |
| Npgsql | Integração PostgreSQL |
| HTML5 | Frontend |
| CSS3 | Interface |
| JavaScript | Frontend |
| Docker | Infraestrutura |
| Docker Compose | Desenvolvimento local |
| GitHub Actions | CI/builds |
| GitHub Pages | Frontend publicado |
| Android WebView | Aplicativo Android atual |
| Java 17 | Build Android |
| Gradle | Build Android |
| Android SDK 35 | Build Android |

---

# 📂 Documentação

Documentos atualmente disponíveis:

- `docs/architecture.md` — arquitetura do projeto.
- `docs/android-webview.md` — funcionamento e build da versão Android.

---

# 🚀 Desenvolvimento local

## PostgreSQL

```bash
docker compose up -d
```

## Backend

A API está em:

```text
src/FinanceFlow.Api
```

## Frontend

A aplicação web principal está em:

```text
index.html
```

A autenticação possui telas próprias em:

```text
frontend/auth.html
frontend/reset-password.html
```

---

# 📜 Histórico de evolução

O FinanceFlow passou por várias etapas de evolução, incluindo:

- criação da aplicação web;
- criação da API ASP.NET Core;
- integração com PostgreSQL;
- estrutura de autenticação;
- criação do Dashboard;
- criação do módulo de transações;
- edição e exclusão de transações;
- filtros de movimentações;
- criação de contas;
- criação de categorias;
- criação de orçamentos;
- relatórios mensais;
- correção dos cálculos financeiros;
- separação entre dinheiro operacional e investimentos;
- criação da identidade visual;
- implementação do tema claro/escuro;
- criação do Android WebView;
- automação da geração do APK;
- correções recentes de JavaScript e da interface principal.

---

# 📋 Resumo executivo

### Já temos

```text
🟢 Dashboard
🟢 Transações
🟢 Filtros
🟢 Contas
🟢 Categorias
🟢 Orçamentos
🟢 Relatórios
🟢 Fluxo financeiro
🟢 Separação de investimentos
🟢 Tema claro/escuro
🟢 Interface responsiva
🟢 Android WebView
🟢 Build automatizado do APK
```

### Falta para o núcleo ficar realmente completo

```text
🔴 Transferências
🔴 Cartões e faturas
🔴 Recorrências
🔴 Calendário financeiro
🔴 Metas
🟠 Investimentos completos
🟠 Histórico patrimonial
🟠 Relatórios avançados
🟡 Importação de extratos
🟡 Notificações
🟡 Android nativo/multiplataforma
```

---

# 👨‍💻 Desenvolvimento

Projeto desenvolvido por **KaykyFerro**.

O FinanceFlow continua em desenvolvimento, com foco em construir uma plataforma financeira pessoal completa, consistente e escalável.

---

# 📄 Licença

Nenhuma licença de código aberto foi definida para o projeto até o momento.
