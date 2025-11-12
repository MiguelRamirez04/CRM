using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) { _config = config;  }
    
    public async Task EnviarRecuperacionCuentaAsync(string destinatario, string token)
    {
        var smtp = _config.GetSection("SmtpSettings");
        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(smtp["SenderName"], smtp["SenderEmail"]));
        mensaje.To.Add(new MailboxAddress("", destinatario));
        mensaje.Subject = "Recuperacion de Cuenta";
        mensaje.Body = new TextPart("plain") { Text = $"Usa este enlace para recuperar tu cuenta: https://api/Auth/change-password?token={token}" };
        mensaje.Body = new TextPart("plain") { Text = $"este es un mensaje automatizado del sistema administrativo del crm de ©Cabs-durango" };
        var htmlBody = $@"
        <html>
        <body>
            <h2>Recuperación de cuenta</h2>
            <p>Haz clic en el siguiente enlace para recuperar tu cuenta:</p>
            <a href='https://api/Auth/change-password?token={token}'>Recuperar cuenta</a>
            <br><br>
            <small>Este es un mensaje automatizado del sistema administrativo del CRM de ©Cabs-durango</small>
        </body>
        </html>";

        mensaje.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        int port = int.TryParse(smtp["Port"], out var parsedPort) ? parsedPort : 587;
        await client.ConnectAsync(smtp["Server"], port, false);
        await client.AuthenticateAsync(smtp["Username"], smtp["password"]);
        await client.SendAsync(mensaje);
        await client.DisconnectAsync(true);
    }
}