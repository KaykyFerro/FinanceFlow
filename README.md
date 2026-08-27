# FinanceFlow

Sistema de gestão financeira pessoal, com frontend web e API REST em ASP.NET Core + PostgreSQL.

> **Status atual:** em desenvolvimento ativo. O núcleo financeiro já está funcional e o módulo de cartões de crédito começou a ser implementado diretamente no backend, banco e frontend auxiliar.

## 🎯 Objetivo

O FinanceFlow centraliza contas, transações, categorias, orçamentos, investimentos e cartões de crédito em uma única aplicação, com separação de dados por usuário.

## 🧱 Stack

- **Backend:** C# / ASP.NET Core
- **ORM:** Entity Framework Core
- **Banco:** PostgreSQL / Npgsql
- **Autenticação:** JWT + refresh tokens
- **Frontend:** HTML, CSS e JavaScript vanilla
- **Deploy:** Docker / Railway para API e GitHub Pages para frontend
- **Arquitetura:** separação entre API, Domain e Infrastructure

## 📁 Estrutura principal

```text
FinanceFlow/
├── src/
│   ├── FinanceFlow.Api/
│   │   ├── Authentication/
│   │   └── Controllers/
│   ├── FinanceFlow.Domain/
│   │   └── Entities/
│   └── FinanceFlow.Infrastructure/
│       ├── Data/
│       └── Entities/
├── frontend/
│   ├── auth.html
│   ├── reset-password.html
│   └── credit-cards.html
├── database/
├── docs/
├── index.html
├── Dockerfile
└── docker-compose.yml
```

## ✅ O que já existe

### 📊 Dashboard

- resumo mensal;
- receitas;
- despesas;
- patrimônio operacional;
- investimentos separados do fluxo de caixa;
- contas e saldos;
- histórico de transações;
- gráfico por dia;
- visão de categorias e orçamentos.

### 💸 Transações

- criação;
- edição;
- exclusão;
- receita e despesa;
- conta vinculada;
- categoria opcional;
- data;
- observações;
- filtros por período, categoria e tipo.

### 🏦 Contas

- cadastro;
- instituição;
- nome;
- tipo de conta;
- saldo inicial;
- contas correntes, poupança, carteira, dinheiro e investimentos;
- proteção por usuário;
- bloqueio de exclusão quando existem transações vinculadas.

### 🏷️ Categorias

- categorias de receita e despesa;
- cor personalizada;
- proteção contra exclusão quando existem transações vinculadas;
- categorias padrão criadas automaticamente para novos usuários.

### 💰 Orçamentos

- limite mensal por categoria;
- cálculo de gasto realizado;
- atualização do limite;
- exclusão;
- unicidade por usuário, categoria e mês.

### 📈 Relatórios / fluxo financeiro

- histórico completo;
- separação entre fluxo operacional e investimentos;
- entradas e saídas agrupadas por dia;
- acompanhamento de patrimônio operacional;
- visão de investimentos separada para evitar dupla contagem.

### 📈 Investimentos

- contas do tipo investimento;
- rendimento configurável no domínio;
- tipos de rendimento previstos: poupança, CDI percentual, taxa fixa anual e IPCA+;
- investimentos não são tratados como receita/despesa operacional comum.

### 🌙 Interface

- tema claro;
- tema escuro;
- tema do sistema;
- layout responsivo;
- navegação desktop e mobile;
- identidade visual FinanceFlow.

### 👤 Sessões de conta

- estrutura para múltiplas contas autenticadas no frontend;
- troca de conta;
- adição de outra conta;
- tokens separados por conta;
- atualização de access/refresh token por sessão.

> A autenticação/multi-conta não é prioridade desta etapa do roadmap.

---

# 💳 Cartões de crédito

## Status: 🟡 Em implementação

Este é o próximo grande módulo financeiro do projeto.

A primeira camada já foi implementada no backend, persistência e em uma tela frontend auxiliar.

### Já implementado

#### Cadastro do cartão

- instituição;
- nome do cartão;
- limite total;
- últimos 4 dígitos;
- dia de fechamento;
- dia de vencimento;
- ativação/inativação;
- isolamento por usuário.

#### Compras

- compra vinculada ao cartão;
- descrição;
- valor total;
- quantidade de parcelas de 1 a 120;
- data da compra;
- primeira fatura configurável;
- categoria opcional;
- observações;
- cálculo da parcela;
- distribuição automática das parcelas entre as faturas futuras.

#### Faturas

- mês de referência;
- fechamento;
- vencimento;
- valor total;
- valor pago;
- saldo restante;
- status aberta, fechada, paga ou vencida;
- histórico das faturas;
- itens de cada fatura;
- identificação da parcela, por exemplo `2/10`.

#### Limite

O módulo já calcula:

```text
Limite total
- valores de faturas atuais/futuras ainda não pagas
= limite disponível
```

O limite considera as compras parceladas futuras para evitar que o cartão pareça ter limite disponível que, na prática, já está comprometido.

#### Pagamento de fatura

O pagamento de uma fatura já pode:

1. reduzir o saldo da fatura;
2. alterar o status para paga quando quitada;
3. registrar uma transação de despesa na conta usada para o pagamento;
4. refletir o pagamento no fluxo de caixa.

Se nenhuma conta for informada pela interface, a API usa automaticamente a primeira conta operacional disponível do usuário. A interface principal ainda deverá evoluir para permitir a escolha explícita da conta de pagamento.

## API de cartões

Base:

```text
/api/credit-cards
```

Endpoints atuais:

```text
GET    /api/credit-cards
GET    /api/credit-cards/{id}
POST   /api/credit-cards
PUT    /api/credit-cards/{id}
DELETE /api/credit-cards/{id}

GET    /api/credit-cards/{id}/invoices
POST   /api/credit-cards/{id}/purchases
POST   /api/credit-cards/{id}/invoices/{invoiceId}/pay
```

## Banco de dados de cartões

Foram adicionadas as entidades:

```text
CreditCard
CreditCardPurchase
CreditCardInvoice
```

E as tabelas:

```text
CreditCards
CreditCardPurchases
CreditCardInvoices
```

A inicialização da aplicação cria essas tabelas automaticamente quando necessário, mantendo o mecanismo atual de inicialização do banco.

## Frontend auxiliar

Existe uma primeira interface funcional em:

```text
frontend/credit-cards.html
```

Ela permite:

- cadastrar cartão;
- visualizar limite utilizado/disponível;
- lançar compra;
- definir parcelamento;
- visualizar histórico de faturas;
- abrir os itens de cada fatura;
- registrar pagamento.

### Próxima integração do frontend

A tela auxiliar ainda precisa ser incorporada à navegação principal do FinanceFlow como um módulo oficial de **Cartões**, incluindo:

- botão no menu lateral;
- cards dos cartões na interface principal;
- modal de cadastro/edição;
- tela de detalhes do cartão;
- seleção da conta utilizada para pagamento;
- melhor visualização das parcelas;
- integração com o Dashboard;
- impacto das faturas futuras no calendário/fluxo de caixa.

---

# 🔴 Próximas prioridades

## 1. Cartões de crédito

### Em andamento

- [x] modelo de cartão;
- [x] limite total;
- [x] limite disponível;
- [x] fechamento;
- [x] vencimento;
- [x] compras;
- [x] compras parceladas;
- [x] geração lógica das faturas futuras;
- [x] histórico de faturas;
- [x] status de fatura;
- [x] pagamento;
- [x] impacto do pagamento no fluxo de caixa;
- [x] tela frontend inicial;
- [ ] integração completa ao frontend principal;
- [ ] edição de compra;
- [ ] cancelamento/estorno de compra;
- [ ] antecipação de parcelas;
- [ ] escolha da conta no pagamento;
- [ ] fechamento manual de fatura;
- [ ] suporte completo a compras após o fechamento;
- [ ] arredondamento da última parcela para garantir que a soma das parcelas seja exatamente igual ao total;
- [ ] integração visual com Dashboard e Relatórios;
- [ ] calendário de faturas.

## 2. Transferências entre contas

- [ ] transferência como operação própria;
- [ ] conta origem;
- [ ] conta destino;
- [ ] valor neutro para patrimônio;
- [ ] histórico de transferência;
- [ ] evitar dupla contagem em receitas/despesas.

## 3. Recorrências

- [ ] receitas recorrentes;
- [ ] despesas recorrentes;
- [ ] periodicidade;
- [ ] geração automática;
- [ ] recorrências futuras.

## 4. Calendário financeiro

- [ ] vencimentos;
- [ ] faturas;
- [ ] recorrências;
- [ ] previsão diária de saldo;
- [ ] visão mensal.

## 5. Metas financeiras

- [ ] criação de metas;
- [ ] valor-alvo;
- [ ] prazo;
- [ ] progresso;
- [ ] contribuições;
- [ ] projeção.

## 6. Investimentos avançados

- [ ] ativos;
- [ ] aportes;
- [ ] resgates;
- [ ] rentabilidade;
- [ ] preço médio;
- [ ] evolução patrimonial;
- [ ] integração com fluxo de caixa.

## 7. Relatórios avançados

- [ ] comparação mensal;
- [ ] evolução patrimonial;
- [ ] despesas por categoria;
- [ ] evolução de orçamento;
- [ ] gastos com cartões;
- [ ] exportação.

## 8. Importação bancária

- [ ] CSV;
- [ ] OFX;
- [ ] categorização automática;
- [ ] prevenção de duplicidade;
- [ ] conciliação.

## 9. Notificações

- [ ] vencimento de fatura;
- [ ] orçamento próximo do limite;
- [ ] conta recorrente;
- [ ] saldo baixo;
- [ ] metas.

---

# 🔐 Segurança e isolamento

As entidades financeiras carregam `UserId` e as consultas da API são filtradas pelo usuário autenticado.

O objetivo é manter o isolamento:

```text
Usuário A
├── Contas
├── Transações
├── Categorias
├── Orçamentos
├── Investimentos
└── Cartões / Faturas / Compras

Usuário B
├── Contas
├── Transações
├── Categorias
├── Orçamentos
├── Investimentos
└── Cartões / Faturas / Compras
```

Nenhum usuário deve conseguir consultar ou alterar recursos financeiros pertencentes a outro usuário.

---

# 🚀 Execução

## API

```bash
dotnet restore
dotnet build
dotnet run --project src/FinanceFlow.Api
```

A API utiliza PostgreSQL e pode receber a conexão pela variável:

```text
DATABASE_URL
```

## Docker

```bash
docker compose up --build
```

## Frontend

O frontend principal está no `index.html`.

A página inicial do módulo de cartões está em:

```text
frontend/credit-cards.html
```

---

# 📌 Estado do projeto

| Área | Estado |
|---|---|
| Dashboard | 🟢 Funcional |
| Transações | 🟢 Funcional |
| Contas | 🟢 Funcional |
| Categorias | 🟢 Funcional |
| Orçamentos | 🟢 Funcional |
| Relatórios básicos | 🟢 Funcional |
| Investimentos básicos | 🟢 Funcional |
| Tema claro/escuro | 🟢 Funcional |
| Autenticação | 🟢 Implementada |
| Multi-conta | 🟡 Implementação recente |
| Cartões - backend | 🟢 Implementado |
| Cartões - banco | 🟢 Implementado |
| Cartões - compras parceladas | 🟢 Implementado |
| Cartões - faturas | 🟢 Implementado |
| Cartões - pagamento | 🟢 Implementado |
| Cartões - fluxo de caixa | 🟢 Implementado |
| Cartões - frontend auxiliar | 🟢 Implementado |
| Cartões - integração no app principal | 🟡 Próximo passo |
| Transferências | 🔴 Pendente |
| Recorrências | 🔴 Pendente |
| Calendário financeiro | 🔴 Pendente |
| Metas | 🔴 Pendente |
| Investimentos avançados | 🟠 Parcial |
| Relatórios avançados | 🔴 Pendente |
| Importação OFX/CSV | 🔴 Pendente |
| Notificações | 🔴 Pendente |

---

## 🛠️ Princípio de desenvolvimento

Cada nova funcionalidade deve atualizar simultaneamente:

1. domínio;
2. persistência;
3. API;
4. frontend;
5. documentação do README;
6. checklist de progresso.

O README é tratado como o mapa vivo do projeto, e não como documentação histórica.
