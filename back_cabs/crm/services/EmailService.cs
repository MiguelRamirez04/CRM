using MailKit.Net.Smtp;
using MimeKit;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using System.Text;

public class EmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) { _config = config; }
    
    /*public async Task EnviarRecuperacionCuentaAsync(string destinatario, string token)
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
    }*/
    public async Task EnviarTokenRecuperacionAsync(string destinatario, string token)
    {
        var gmailSettings = _config.GetSection("GmailSettings");
        bool useGmailApi = gmailSettings.GetValue<bool>("UseGmailApi");

        if (useGmailApi)
        {
            await EnviarEmailViaGmailApiAsync(destinatario, "Recuperación de cuenta", GenerarCuerpoHtmlRecuperacion(token, destinatario));
            return;
        }

        // Modo SMTP existente
        var smtp = _config.GetSection("SmtpSettings");
        var server = smtp["Server"];
        var username = smtp["Username"];
        var password = smtp["Password"];

        // Para testing: si son credenciales de prueba, no enviar email real, solo loguear
        if (server == "smtp.cabs_pruebas.com" && username == "Usuario" && password == "contraseña")
        {
            Console.WriteLine($"[TEST MODE] Token de recuperación para {destinatario}: {token}");
            return; // No enviar email real
        }

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(smtp["SenderName"], smtp["SenderEmail"]));
        mensaje.To.Add(new MailboxAddress("", destinatario));
        mensaje.Subject = "Recuperación de cuenta";
        var htmlBody = GenerarCuerpoHtmlRecuperacion(token, destinatario);
        mensaje.Body = new TextPart("html") { Text = htmlBody };
        using var client = new SmtpClient();
        int port = int.TryParse(smtp["Port"], out var parsePort) ? parsePort : 587;
        await client.ConnectAsync(server, port, false);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(mensaje);
        await client.DisconnectAsync(true);
    }

    private string GenerarCuerpoHtmlRecuperacion(string token, string destinatario)
    {
        return $@"
        <html>
        <body>
            <h2>Recuperación de cuenta</h2>
            <p>Tu código de recuperación es: <b>{token}</b></p>
            <p>O haz clic en el siguiente enlace para cambiar tu contraseña:</p>
            <a href='https://frontend-app/cambiar-contraseña?email={destinatario}&token={token}'>Cambiar contraseña</a>
            <br><br>
            <small>Este es un mensaje automatizado del CRM ©Cabs-durango</small>
        </body>
        </html>
        ";
    }

    private async Task EnviarEmailViaGmailApiAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        var gmailSettings = _config.GetSection("GmailSettings");
        var clientId = gmailSettings["ClientId"];
        var clientSecret = gmailSettings["ClientSecret"];
        var refreshToken = gmailSettings["RefreshToken"];
        var senderEmail = gmailSettings["SenderEmail"];

        // Crear credenciales OAuth 2.0
        var credential = GoogleCredential.FromAccessToken("") // Necesitamos obtener el access token
            .CreateScoped(GmailService.Scope.GmailSend);

        // Para simplificar, usaremos refresh token directamente
        var credentials = new UserCredential(
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                }
            }),
            "user",
            new TokenResponse { RefreshToken = refreshToken }
        );

        // Crear servicio de Gmail
        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credentials,
            ApplicationName = "CRM Cabs"
        });

        // Crear mensaje
        var mensaje = new Message();
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("CRM Cabs", senderEmail));
        mimeMessage.To.Add(new MailboxAddress("", destinatario));
        mimeMessage.Subject = asunto;
        mimeMessage.Body = new TextPart("html") { Text = cuerpoHtml };

        // Convertir a formato RFC 2822
        using var stream = new MemoryStream();
        await mimeMessage.WriteToAsync(stream);
        var rawMessage = Convert.ToBase64String(stream.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

        mensaje.Raw = rawMessage;

        // Enviar email
        await service.Users.Messages.Send(mensaje, "me").ExecuteAsync();
    }
}

