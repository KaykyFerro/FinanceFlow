# Arquitetura do FinanceFlow

## Stack

- Frontend: Flutter
- API: ASP.NET Core Web API / .NET 10
- Persistência: Entity Framework Core + PostgreSQL
- Autenticação: JWT + Refresh Token

## Domínio inicial

- Usuário
- Conta financeira
- Instituição financeira
- Tipo de conta
- Tipo de rendimento

## Contas e rendimentos

Uma conta pertence a um usuário e pode representar conta corrente, poupança, carteira, dinheiro físico ou investimento.

O rendimento não fica preso a um banco específico. Ele é configurável por conta para suportar, por exemplo:

- sem rendimento
- poupança
- percentual do CDI, como 100% ou 120%
- taxa fixa anual
- IPCA + percentual

Instituições iniciais previstas: Nubank, Santander, Itaú, Banco do Brasil, Bradesco, Caixa, Inter, Mercado Pago, PicPay, PagBank, C6 Bank, Neon, BTG Pactual, XP, Rico e outras personalizadas.

## Segurança

As credenciais serão armazenadas somente como hash de senha. Os dados financeiros serão sempre associados ao usuário autenticado e isolados por usuário.

## Próxima etapa

Implementar autenticação, persistência com PostgreSQL e o fluxo de cadastro/login antes de avançar para o dashboard completo.
