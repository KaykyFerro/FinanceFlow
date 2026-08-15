using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace FinanceFlow.Api.Authentication;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = "https://kaykyferro.github.io/FinanceFlow";
    public bool EnableSsl { get; set; } = true;
}

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public Task SendVerificationAsync(string email, string token, CancellationToken cancellationToken)
        => SendAsync(email, "Confirme seu e-mail | FinanceFlow", BuildVerificationHtml(token), cancellationToken);

    public Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken)
        => SendAsync(email, "Redefinição de senha | FinanceFlow", BuildResetHtml(token), cancellationToken);

    private async Task SendAsync(string email, string subject, string html, CancellationToken cancellationToken)
    {
        ValidateConfiguration();
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Starting FinanceFlow SMTP delivery. Host={Host}, Port={Port}, Username={Username}, From={From}, Recipient={Recipient}, Ssl={Ssl}",
            _options.Host, _options.Port, _options.Username, _options.From, email, _options.EnableSsl);

        try
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.From, "FinanceFlow"),
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(email));

            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("FinanceFlow SMTP delivery completed successfully for {Recipient}", email);
        }
        catch (SmtpException ex)
        {
            logger.LogError(ex,
                "FinanceFlow SMTP delivery failed. Host={Host}, Port={Port}, Username={Username}, From={From}, Recipient={Recipient}, Ssl={Ssl}, StatusCode={StatusCode}",
                _options.Host, _options.Port, _options.Username, _options.From, email, _options.EnableSsl, ex.StatusCode);
            throw new InvalidOperationException("Não foi possível enviar o e-mail de confirmação. O serviço de e-mail está temporariamente indisponível.", ex);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            logger.LogError(ex, "FinanceFlow SMTP configuration/address error for recipient {Recipient}", email);
            throw new InvalidOperationException("A configuração do serviço de e-mail é inválida.", ex);
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            _options.Port <= 0 ||
            string.IsNullOrWhiteSpace(_options.Username) ||
            string.IsNullOrWhiteSpace(_options.Password) ||
            string.IsNullOrWhiteSpace(_options.From))
        {
            logger.LogError(
                "SMTP configuration is incomplete. HostConfigured={HostConfigured}, Port={Port}, UsernameConfigured={UsernameConfigured}, PasswordConfigured={PasswordConfigured}, FromConfigured={FromConfigured}",
                !string.IsNullOrWhiteSpace(_options.Host), _options.Port,
                !string.IsNullOrWhiteSpace(_options.Username), !string.IsNullOrWhiteSpace(_options.Password),
                !string.IsNullOrWhiteSpace(_options.From));
            throw new InvalidOperationException("SMTP não está configurado. Configure Smtp__Host, Smtp__Port, Smtp__Username, Smtp__Password e Smtp__From.");
        }
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
