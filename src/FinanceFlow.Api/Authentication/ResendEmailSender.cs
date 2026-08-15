using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace FinanceFlow.Api.Authentication;

public sealed class ResendOptions
{
    public const string SectionName = "Resend";
    public string ApiKey { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = "https://kaykyferro.github.io/FinanceFlow";
}

public sealed class ResendEmailSender(
    IOptions<ResendOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    private readonly ResendOptions _options = options.Value;

    public Task SendVerificationAsync(string email, string token, CancellationToken cancellationToken)
        => SendAsync(email, "Confirme seu e-mail | FinanceFlow", BuildVerificationHtml(token), cancellationToken);

    public Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken)
        => SendAsync(email, "Redefinição de senha | FinanceFlow", BuildResetHtml(token), cancellationToken);

    private async Task SendAsync(string email, string subject, string html, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.From))
            throw new InvalidOperationException("Resend não está configurado. Configure Resend__ApiKey e Resend__From.");

        var client = httpClientFactory.CreateClient("Resend");
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_options.ApiKey}");

        logger.LogInformation("Sending FinanceFlow email via Resend to {Recipient}", email);

        using var response = await client.PostAsJsonAsync(
            "emails",
            new
            {
                from = _options.From,
                to = new[] { email },
                subject,
                html
            },
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Resend rejected FinanceFlow email. Status={StatusCode}, Response={Response}", response.StatusCode, body);
            throw new InvalidOperationException("Não foi possível enviar o e-mail agora. O serviço de e-mail recusou a solicitação.");
        }

        logger.LogInformation("FinanceFlow email sent successfully via Resend to {Recipient}", email);
    }

    private string BuildVerificationHtml(string token)
    {
        var link = $"{_options.FrontendUrl.TrimEnd('/')}/verify-email.html?token={Uri.EscapeDataString(token)}";
        return $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;color:#10213f">
          <h1>FinanceFlow</h1>
          <p>Seu cadastro foi criado com sucesso.</p>
          <p>Clique no botão abaixo para confirmar seu e-mail:</p>
          <p><a href="{link}" style="display:inline-block;background:#0aa579;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;font-weight:bold">Confirmar meu e-mail</a></p>
          <p>Este link expira em 24 horas.</p>
          <p style="color:#697386;font-size:12px">Se você não criou esta conta, ignore este e-mail.</p>
        </div>
        """;
    }

    private string BuildResetHtml(string token)
    {
        var link = $"{_options.FrontendUrl.TrimEnd('/')}/auth.html?resetToken={Uri.EscapeDataString(token)}";
        return $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;color:#10213f">
          <h1>FinanceFlow</h1>
          <p>Recebemos uma solicitação para redefinir sua senha.</p>
          <p><a href="{link}" style="display:inline-block;background:#0aa579;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;font-weight:bold">Redefinir minha senha</a></p>
          <p>Este link expira em 30 minutos.</p>
          <p style="color:#697386;font-size:12px">Se você não solicitou a redefinição, ignore este e-mail.</p>
        </div>
        """;
    }
}
