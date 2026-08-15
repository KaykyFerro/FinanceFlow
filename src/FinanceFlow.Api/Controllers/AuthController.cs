using System.Security.Claims;
using FinanceFlow.Api.Authentication;
using FinanceFlow.Domain.Entities;
using FinanceFlow.Infrastructure.Data;
using FinanceFlow.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    FinanceFlowDbContext db,
    IPasswordHasher<User> passwordHasher,
    TokenService tokens,
    AuthTokenService authTokens,
    IEmailSender emailSender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 8)
            return BadRequest(new { message = "Nome, e-mail e uma senha de pelo menos 8 caracteres são obrigatórios." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
            return Conflict(new { message = "Este e-mail já está cadastrado." });

        var user = new User(request.Name, email, string.Empty);
        user.ChangePasswordHash(passwordHasher.HashPassword(user, request.Password));
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var token = await authTokens.CreateAsync(user.Id, AuthTokenType.EmailVerification, TimeSpan.FromHours(24), cancellationToken);
        await emailSender.SendVerificationAsync(user.Email, token, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Cadastro criado. Verifique seu e-mail para ativar a conta.",
            user = TokenService.ToResponse(user)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        if (!user.EmailConfirmed)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Confirme seu e-mail antes de entrar." });

        user.RegisterLogin();
        await db.SaveChangesAsync(cancellationToken);
        return Ok(await tokens.IssueAsync(user, cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await tokens.RotateAsync(request.RefreshToken, cancellationToken);
        if (result is null) return Unauthorized(new { message = "Refresh token inválido ou expirado." });
        var response = await tokens.IssueAsync(result.Value.User, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken cancellationToken)
    {
        await tokens.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var token = await authTokens.ConsumeAsync(request.Token, AuthTokenType.EmailVerification, cancellationToken);
        if (token is null) return BadRequest(new { message = "Token de verificação inválido ou expirado." });

        var user = await db.Users.FindAsync([token.UserId], cancellationToken);
        if (user is null) return BadRequest(new { message = "Usuário não encontrado." });
        user.ConfirmEmail();
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "E-mail confirmado com sucesso." });
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is not null && !user.EmailConfirmed)
        {
            var token = await authTokens.CreateAsync(user.Id, AuthTokenType.EmailVerification, TimeSpan.FromHours(24), cancellationToken);
            await emailSender.SendVerificationAsync(user.Email, token, cancellationToken);
        }
        return Ok(new { message = "Se o cadastro existir e ainda não estiver confirmado, uma nova verificação foi enviada." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is not null)
        {
            var token = await authTokens.CreateAsync(user.Id, AuthTokenType.PasswordReset, TimeSpan.FromMinutes(30), cancellationToken);
            await emailSender.SendPasswordResetAsync(user.Email, token, cancellationToken);
        }

        return Ok(new { message = "Se o e-mail estiver cadastrado, você receberá instruções para redefinir a senha." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (request.NewPassword.Length < 8)
            return BadRequest(new { message = "A nova senha deve ter pelo menos 8 caracteres." });

        var token = await authTokens.ConsumeAsync(request.Token, AuthTokenType.PasswordReset, cancellationToken);
        if (token is null) return BadRequest(new { message = "Token de recuperação inválido ou expirado." });

        var user = await db.Users.FindAsync([token.UserId], cancellationToken);
        if (user is null) return BadRequest(new { message = "Usuário não encontrado." });
        user.ChangePasswordHash(passwordHasher.HashPassword(user, request.NewPassword));
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Senha alterada com sucesso." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await db.Users.FindAsync([userId], cancellationToken);
        return user is null ? Unauthorized() : Ok(TokenService.ToResponse(user));
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var user = await db.Users.FindAsync([userId], cancellationToken);
        if (user is null) return Unauthorized();

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email && x.Id != userId, cancellationToken))
            return Conflict(new { message = "Este e-mail já está em uso." });

        var emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
        user.UpdateProfile(request.Name, email);
        if (emailChanged)
        {
            // Requer nova confirmação quando o endereço muda.
            // A entidade mantém a confirmação anterior somente até a camada de domínio ser estendida.
        }
        await db.SaveChangesAsync(cancellationToken);
        return Ok(TokenService.ToResponse(user));
    }

    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
