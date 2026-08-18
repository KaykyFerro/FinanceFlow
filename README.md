# FinanceFlow

Sistema de gestão financeira pessoal desenvolvido para centralizar receitas, despesas, contas, investimentos e análises financeiras em uma única aplicação.

> **Status atual:** 🚧 Em desenvolvimento ativo. O projeto já possui uma interface web funcional, estrutura de API/backend, PostgreSQL, autenticação em evolução, controle de transações, relatórios, identidade visual própria e uma versão Android baseada em WebView com build automatizado.

## 🌐 Acesso

- **Aplicação web:** https://kaykyferro.github.io/FinanceFlow/
- **Tela de autenticação:** https://kaykyferro.github.io/FinanceFlow/frontend/auth.html
- **Repositório:** https://github.com/KaykyFerro/FinanceFlow

---

## 🎯 Objetivo

O FinanceFlow tem como objetivo oferecer um sistema simples e completo para controle financeiro pessoal, permitindo registrar movimentações, acompanhar o fluxo de caixa, separar dinheiro disponível de investimentos e visualizar relatórios por período.

A aplicação foi estruturada para evoluir de um protótipo web para uma solução completa com backend, banco de dados e aplicativo mobile.

---

## 🧩 Funcionalidades implementadas

### 🔐 Autenticação

- Tela de login.
- Tela de criação de conta.
- Fluxo de recuperação de senha.
- Estrutura de autenticação integrada ao projeto backend.
- Modelos de requisição e resposta para autenticação.
- Preparação para autenticação com JWT.
- Proteção das áreas autenticadas da aplicação.
- Estrutura de persistência de usuários no PostgreSQL.

> A camada de autenticação ainda está em evolução e algumas integrações dependem da API/backend disponível no ambiente de execução.

### 📊 Dashboard financeiro

O dashboard apresenta uma visão geral das finanças do período selecionado, incluindo:

- Entradas do mês.
- Saídas do mês.
- Patrimônio.
- Investimentos.
- Fluxo de caixa.
- Contas e respectivos saldos.
- Navegação entre meses.
- Botão de criação de nova transação.

### 💰 Separação entre patrimônio e investimentos

A regra financeira foi ajustada para que investimentos não sejam contabilizados como dinheiro disponível para gastos.

- **Investimentos** são exibidos separadamente.
- O saldo investido não é somado ao patrimônio disponível para movimentações correntes.
- O cálculo do patrimônio foi corrigido para excluir os valores classificados como investimentos.
- O dashboard apresenta os valores de patrimônio e investimentos em indicadores separados.

Essa separação evita que um valor aplicado seja interpretado pelo sistema como dinheiro disponível para despesas.

### 💸 Transações

A tela de transações foi ampliada para funcionar como um histórico geral, independentemente do mês atualmente selecionado no dashboard.

Implementado:

- Exibição de todas as transações cadastradas.
- Filtro por data inicial.
- Filtro por data final.
- Filtro por categoria.
- Filtro por tipo de movimentação.
- Opção para limpar os filtros.
- Exibição da quantidade total de movimentações cadastradas.
- Identificação visual de entradas e saídas.
- Exibição de data, descrição, categoria, conta, tipo e valor.
- Edição de transações.
- Exclusão de transações.

### 📈 Relatórios

A área de relatórios recebeu controle próprio de período, permitindo analisar os dados do mês selecionado.

Inclui:

- Seletor de mês.
- Navegação entre meses.
- Total de entradas do período.
- Total de saídas do período.
- Saldo do mês.
- Gastos por categoria.
- Fluxo diário.
- Mensagens de estado quando não existem transações no período.

O seletor de mês foi corrigido para acompanhar corretamente as transações cadastradas.

### 🎨 Tema claro e escuro

Foi implementado suporte à alternância entre tema claro e escuro.

Também foi adicionado comportamento baseado na preferência do dispositivo/navegador:

- A aplicação pode detectar `prefers-color-scheme`.
- A tela de autenticação possui o controle de tema.
- O tema é aplicado sem depender exclusivamente do dashboard.
- A preferência escolhida pode ser mantida pelo navegador.

### 🪪 Identidade visual

O FinanceFlow recebeu uma identidade visual própria, incluindo:

- Nova logo oficial.
- Aplicação da logo na tela de autenticação.
- Aplicação da logo no menu lateral.
- Correção do dimensionamento para evitar distorção.
- Nome **FinanceFlow** integrado à identidade visual.
- Interface com foco em tons escuros, verde para entradas e azul para indicadores financeiros.

### 📱 Aplicativo Android WebView

Foi criada e documentada uma versão Android baseada em WebView para carregar a aplicação web do FinanceFlow.

Características:

- Nome do aplicativo: **FinanceFlow**.
- Package: `com.kaykyferro.financeflow`.
- WebView apontando para a tela de autenticação publicada.
- JavaScript e DOM Storage habilitados.
- Navegação de retorno do Android.
- HTTPS para acesso à aplicação web.
- Build automatizado pelo GitHub Actions.
- APK de Release publicado como artifact `FinanceFlow-APK`.
- Documentação específica em [`docs/android-webview.md`](docs/android-webview.md).

### 📥 Download do APK Android

O APK é gerado automaticamente pelo GitHub Actions. Para baixar o build mais recente:

**[📱 Abrir builds do Android e baixar o APK](https://github.com/KaykyFerro/FinanceFlow/actions/workflows/android.yml)**

Na execução mais recente concluída com sucesso, abra **Artifacts → FinanceFlow-APK** e baixe o ZIP contendo `FinanceFlow.apk`.

> Os artifacts do GitHub Actions são temporários e o workflow atual mantém cada APK por 30 dias. Para distribuição permanente, o próximo passo recomendado é publicar o APK em uma Release do GitHub.

> A versão Android/WebView ainda deve ser considerada uma versão de teste enquanto o projeto mobile não possuir uma implementação nativa definitiva.

---

## 🏗️ Arquitetura

O projeto está organizado para separar responsabilidades entre domínio, infraestrutura e API:

```text
FinanceFlow/
├── database/
├── docs/
│   ├── architecture.md
│   └── android-webview.md
├── frontend/
│   ├── assets/
│   ├── auth.html
│   └── reset-password.html
├── src/
│   ├── FinanceFlow.Api/
│   ├── FinanceFlow.Domain/
│   └── FinanceFlow.Infrastructure/
├── .github/
│   └── workflows/
│       └── android.yml
├── Dockerfile
├── docker-compose.yml
├── index.html
└── README.md
```

### Backend

- **C# / ASP.NET Core Web API**.
- **.NET 10**.
- Separação entre API, domínio e infraestrutura.
- Preparação para autenticação JWT.
- Entity Framework Core.
- Npgsql para PostgreSQL.

### Banco de dados

- **PostgreSQL**.
- Ambiente local preparado com Docker Compose.
- Volume persistente para os dados.
- Estrutura inicial para autenticação e usuários.
- Integração preparada para ambiente de produção utilizando variável de conexão do banco.

### Infraestrutura

- Docker.
- Docker Compose para PostgreSQL local.
- Dockerfile para publicação da API em ambiente de produção.
- Estrutura preparada para execução em serviços de hospedagem como Railway.
- GitHub Actions para automações do projeto e geração do APK.

---

## 🗄️ PostgreSQL local

O projeto possui um ambiente PostgreSQL definido no `docker-compose.yml`.

Configuração de desenvolvimento atual:

```yaml
POSTGRES_DB: financeflow
POSTGRES_USER: financeflow
POSTGRES_PASSWORD: financeflow_dev
ports:
  - "5432:5432"
```

Para iniciar o banco local:

```bash
docker compose up -d
```

Para interromper os containers:

```bash
docker compose down
```

Os dados são mantidos no volume `financeflow_postgres_data`.

> Em produção, credenciais e URLs de banco não devem ser colocadas diretamente no código. O backend possui suporte à configuração por variável de ambiente.

---

## 🐳 Docker

A API possui um `Dockerfile` multi-stage utilizando .NET 10:

1. Restauração das dependências.
2. Compilação dos projetos.
3. Publicação em modo Release.
4. Execução utilizando a imagem runtime do ASP.NET.

O ambiente de produção utiliza:

```text
ASPNETCORE_ENVIRONMENT=Production
```

---

## 📁 Principais módulos

| Módulo | Situação |
|---|---|
| Autenticação | 🟡 Em evolução |
| Dashboard | 🟢 Implementado |
| Transações | 🟢 Implementado |
| Edição de transações | 🟢 Implementado |
| Exclusão de transações | 🟢 Implementado |
| Filtros de transações | 🟢 Implementado |
| Categorias | 🟡 Em evolução |
| Contas | 🟡 Em evolução |
| Relatórios | 🟢 Implementado |
| Seletor de período nos relatórios | 🟢 Implementado |
| Orçamentos | 🟡 Planejado/em evolução |
| Metas financeiras | 🟡 Planejado |
| Transações recorrentes | 🟡 Planejado |
| Cartões e faturas | 🟡 Planejado |
| Investimentos | 🟢 Regra de separação implementada |
| Calendário financeiro | 🟡 Planejado |
| Tema claro/escuro | 🟢 Implementado |
| Identidade visual | 🟢 Implementado |
| Aplicativo Android WebView | 🟢 Build automatizado |

---

## 🧮 Regras financeiras atuais

O sistema diferencia movimentações financeiras de acordo com sua finalidade.

### Entradas

Valores recebidos, como salário e vale alimentação, são registrados como entradas.

### Saídas

Valores utilizados para despesas são registrados como saídas.

### Investimentos

Valores classificados como investimento possuem tratamento separado para evitar que sejam considerados dinheiro disponível para gastos.

### Patrimônio

O patrimônio exibido no dashboard considera o saldo financeiro disponível conforme as regras atuais do sistema e não incorpora automaticamente os valores classificados como investimentos.

---

## 🎨 Interface

A interface atual utiliza uma abordagem de dashboard financeiro com:

- Menu lateral.
- Cards de indicadores.
- Tabelas para movimentações.
- Filtros.
- Navegação mensal.
- Gráficos/visualizações de fluxo.
- Estados vazios para períodos sem movimentações.
- Feedback visual diferente para entradas e saídas.
- Tema claro e escuro.
- Identidade visual própria do FinanceFlow.

---

## 🚀 Deploy

O frontend possui publicação pelo **GitHub Pages**.

O backend possui estrutura Docker preparada para hospedagem em ambiente de produção e integração com banco PostgreSQL externo.

O projeto também possui automações via **GitHub Actions**, incluindo o processo de geração do APK Android WebView.

---

## 📱 Build Android

O APK é gerado automaticamente pelo workflow `.github/workflows/android.yml`.

Fluxo simplificado:

```text
GitHub
  ↓
Checkout do projeto
  ↓
Configuração Java 17 + Android SDK
  ↓
Criação do projeto Android WebView
  ↓
Compilação Gradle Release
  ↓
FinanceFlow.apk
  ↓
Artifact: FinanceFlow-APK
```

**[📥 Baixar o APK pelo GitHub Actions](https://github.com/KaykyFerro/FinanceFlow/actions/workflows/android.yml)**

O APK de teste é destinado à distribuição manual enquanto o aplicativo mobile definitivo não é implementado.

---

## 🔒 Segurança

A arquitetura prevê:

- JWT Bearer Authentication.
- Hashing de senhas.
- Separação entre frontend, API e banco.
- Configuração de banco por variáveis de ambiente em produção.
- HTTPS no acesso publicado pelo GitHub Pages.
- O aplicativo WebView utiliza HTTPS para acessar o frontend publicado.

**Importante:** o ambiente de desenvolvimento não deve ser utilizado como ambiente de produção sem revisar credenciais, secrets, CORS, JWT, conexão com banco e demais configurações de segurança.

---

## 🛠️ Tecnologias

- C#
- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Npgsql
- JWT Bearer Authentication
- HTML5
- CSS3
- JavaScript
- Docker
- Docker Compose
- GitHub Actions
- GitHub Pages
- Android WebView
- Java 17
- Gradle
- Android SDK 35

---

## 📌 Próximos passos

Alguns pontos ainda previstos para evolução do projeto:

- Finalizar a integração completa entre frontend e API.
- Consolidar autenticação e gerenciamento de sessão.
- Expandir persistência de contas e transações no PostgreSQL.
- Implementar cartões de crédito e faturas.
- Implementar orçamentos.
- Implementar metas financeiras.
- Implementar transações recorrentes.
- Expandir os relatórios e gráficos.
- Melhorar o módulo de investimentos.
- Implementar calendário financeiro.
- Evoluir a versão Android para uma experiência mobile mais completa.
- Publicar o APK em uma Release permanente do GitHub.
- Fortalecer testes automatizados.
- Revisar configurações de produção e segurança.

---

## 📜 Histórico recente de evolução

Entre as alterações recentes do projeto estão:

- Criação da estrutura inicial de autenticação e banco PostgreSQL.
- Implementação dos endpoints de autenticação.
- Proteção do dashboard por autenticação.
- Estrutura Docker para API e PostgreSQL.
- Correções de conexão com banco em produção.
- Implementação da edição de transações.
- Implementação da exibição de todas as transações.
- Adição de filtros por data, categoria e tipo.
- Correção dos cálculos de saldo, patrimônio e investimentos.
- Separação dos investimentos do patrimônio disponível.
- Remoção de scripts/workflows temporários utilizados durante o desenvolvimento da interface.
- Adição do seletor mensal aos relatórios.
- Correções do seletor de período nos relatórios.
- Implementação do tema baseado na preferência do sistema na autenticação.
- Criação e aplicação da nova identidade visual.
- Aplicação da logo na autenticação e no menu lateral.
- Correção da logo para evitar distorções.
- Criação da documentação do Android WebView.
- Criação do workflow automatizado para geração do APK Release.
- Publicação do APK como artifact `FinanceFlow-APK`.

---

## 👨‍💻 Desenvolvimento

Projeto desenvolvido por **KaykyFerro**.

O FinanceFlow está em desenvolvimento contínuo, com foco em transformar a aplicação em uma solução completa de gestão financeira pessoal.

---

## 📄 Licença

Nenhuma licença de código aberto foi definida para o projeto até o momento.
