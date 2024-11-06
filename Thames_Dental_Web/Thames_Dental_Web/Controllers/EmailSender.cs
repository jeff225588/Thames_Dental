using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }



    public async Task SendEmailAsync(string email, string subject, string nombre, string fecha, string hora, string especialidad, string servicio, string especialista, bool isCancellation = false, bool isReschedule = false)
    {
        var clientId = _configuration["GoogleAuthSettings:ClientId"];
        var clientSecret = _configuration["GoogleAuthSettings:ClientSecret"];
        var refreshToken = _configuration["GoogleAuthSettings:RefreshToken"];
        var userEmail = _configuration["GoogleAuthSettings:UserEmail"];

        var tokenResponse = new TokenResponse
        {
            RefreshToken = refreshToken
        };

        var clientSecrets = new ClientSecrets
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        var credential = new UserCredential(new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = clientSecrets
            }), userEmail, tokenResponse);

        try
        {
            await credential.RefreshTokenAsync(CancellationToken.None);
        }
        catch (TokenResponseException ex)
        {
            Console.WriteLine("Error al refrescar el token: " + ex.Message);
            return;
        }

        var gmailService = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Thames Dental",
        });

        // Cambiar el cuerpo HTML en función de si es una cancelación o confirmación
        var htmlBody = isCancellation
            ? $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h1 style='color: #d9534f;'>Cancelación de Cita</h1>
                <p><strong>Hola {nombre},</strong></p>
                <p>Lamentamos informarte que tu cita programada para el <strong>{fecha}</strong> a las <strong>{hora}</strong> ha sido cancelada.</p>
                <p><strong>Especialidad:</strong> {especialidad}</p>
                <p><strong>Servicio:</strong> {servicio}</p>
                <p><strong>Especialista:</strong> {especialista}</p>
                <br />
                <p style='color: #555;'>Gracias por confiar en nosotros. Puedes comunicarte con nosotros si deseas reprogramar tu cita.</p>
            </body>
            </html>":
        isReschedule ?
        $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <h1 style='color: #007bff;'>Cita Reprogramada</h1>
            <p><strong>Hola {nombre},</strong></p>
            <p>Tu cita ha sido reprogramada para el <strong>{fecha}</strong> a las <strong>{hora}</strong>.</p>
            <p><strong>Especialidad:</strong> {especialidad}</p>
            <p><strong>Servicio:</strong> {servicio}</p>
            <p><strong>Especialista:</strong> {especialista}</p>
            <br />
            <p>Gracias por confiar en Thames Dental.</p>
        </body>
        </html>" 
            : $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h1 style='color: #007bff;'>Nueva cita agendada</h1>
                <p><strong>Hola {nombre},</strong></p>
                <p>Tu cita ha sido agendada para el <strong>{fecha}</strong> a las <strong>{hora}</strong>.</p>
                <p><strong>Especialidad:</strong> {especialidad}</p>
                <p><strong>Servicio:</strong> {servicio}</p>
                <p><strong>Especialista:</strong> {especialista}</p>
                <br />
                <p style='color: #555;'>Gracias por confiar en nosotros.</p>
            </body>
            </html>";

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress("Thames Dental", userEmail));
        mimeMessage.To.Add(new MailboxAddress("", email));
        mimeMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using (var stream = new MemoryStream())
        {
            await mimeMessage.WriteToAsync(stream);
            var rawMessage = Convert.ToBase64String(stream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            var messageToSend = new Message
            {
                Raw = rawMessage
            };

            await gmailService.Users.Messages.Send(messageToSend, "me").ExecuteAsync();
        }
    }



}
