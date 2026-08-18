# FinanceFlow Android WebView

A versão Android do FinanceFlow utiliza uma aplicação nativa simples como container WebView para carregar a versão web publicada do sistema.

## Aplicação

- **Nome:** FinanceFlow
- **Package:** `com.kaykyferro.financeflow`
- **Tecnologia:** Android + Java + WebView
- **URL carregada:** `https://kaykyferro.github.io/FinanceFlow/frontend/auth.html`
- **SDK alvo:** Android 35
- **Compatibilidade mínima:** Android 6.0 / API 23
- **Java:** 17

## Comportamento

O WebView foi configurado para:

- Habilitar JavaScript.
- Habilitar DOM Storage.
- Permitir o funcionamento do armazenamento web utilizado pela aplicação.
- Manter navegação interna dentro do aplicativo.
- Permitir voltar pelas páginas anteriores usando o botão de retorno do Android.
- Utilizar HTTPS para acessar o FinanceFlow publicado.
- Exibir o nome FinanceFlow no aplicativo.

## Build automático

O APK é compilado pelo GitHub Actions através do workflow:

`/.github/workflows/android.yml`

O workflow:

1. Baixa o código do repositório.
2. Configura Java 17.
3. Configura o Android SDK.
4. Instala Android 35 e Build Tools 35.0.0.
5. Cria o projeto Android WebView.
6. Compila o APK em modo Release.
7. Renomeia o arquivo para `FinanceFlow.apk`.
8. Publica o APK como artifact `FinanceFlow-APK`.

## 📥 Download do APK

O GitHub Actions mantém o APK gerado como artifact do último build executado com sucesso.

**[Abrir a página de builds e baixar o APK](https://github.com/KaykyFerro/FinanceFlow/actions/workflows/android.yml)**

Na execução mais recente concluída com sucesso:

`Artifacts → FinanceFlow-APK`

> Os artifacts do GitHub Actions são temporários. O workflow atual mantém cada APK por 30 dias. Para distribuição permanente, o próximo passo recomendado é publicar o APK em uma Release do GitHub.

## Instalação

1. Baixe o artifact `FinanceFlow-APK`.
2. Extraia o arquivo ZIP baixado pelo GitHub.
3. Transfira `FinanceFlow.apk` para o dispositivo Android, caso o download tenha sido feito no computador.
4. Instale o APK.
5. Se o Android solicitar, autorize a instalação de aplicativos provenientes da fonte utilizada para o download.

## Observação

Esta versão é um aplicativo WebView e depende da aplicação web publicada. Ela não contém toda a lógica financeira localmente e precisa de conexão com a internet para carregar o FinanceFlow publicado.

A versão WebView é adequada para testes e distribuição inicial. Uma versão Android nativa pode ser desenvolvida posteriormente caso seja necessário maior integração com recursos do dispositivo.
