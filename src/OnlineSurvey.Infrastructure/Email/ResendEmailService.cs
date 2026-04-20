using OnlineSurvey.Application.Interfaces;
using Resend;

namespace OnlineSurvey.Infrastructure.Email;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;

    public ResendEmailService(IResend resend) => _resend = resend;

    public async Task SendAccessCodeAsync(string to, string surveyTitle, string code, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage();
        message.From = "Online Survey <no-reply@luanfernandes.dev>";
        message.To.Add(to);
        message.Subject = $"Seu código de acesso — {surveyTitle}";
        message.HtmlBody = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px">
              <h2 style="color:#6366f1">Online Survey</h2>
              <p>Você solicitou acesso à pesquisa <strong>{surveyTitle}</strong>.</p>
              <p>Use o código abaixo para continuar:</p>
              <div style="font-size:2.5rem;font-weight:700;letter-spacing:0.5rem;color:#6366f1;text-align:center;padding:24px;background:#f5f3ff;border-radius:12px;margin:24px 0">
                {code}
              </div>
              <p style="color:#888;font-size:0.875rem">Este código expira em 15 minutos e é válido para uso único.</p>
            </div>
            """;

        await _resend.EmailSendAsync(message, cancellationToken);
    }
}
