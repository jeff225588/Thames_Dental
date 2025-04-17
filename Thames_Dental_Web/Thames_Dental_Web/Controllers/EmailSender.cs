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
                <h3 style='color: #d9534f;'>Cancelación de Cita</h3>
                <p><strong>Hola {nombre},</strong></p>
                <p>Este correo le informa que la cita programada para el <strong>{fecha}</strong> a las <strong>{hora}</strong> ha sido cancelada.</p>
                   
                    <p><strong>Especialidad:</strong> {especialidad}</p>
                    <p><strong>Servicio:</strong> {servicio}</p>
                    <p><strong>Especialista:</strong> {especialista}</p>
                <br />

                <p style='color: #555;'>Puedes comunicarte con nosotros si deseas reprogramar la cita.</p>
            </body>
            </html>":
        isReschedule ?
        $@"
        <html>
        <body style='font-family: Arial, sans-serif;'>
            <h3 style='color: #007bff;'>Cita Reprogramada</h3>
            <p><strong>Estimado(a) {nombre},</strong></p>
            <p>Tu cita ha sido reprogramada! Los detalles de tu cita son: </p>
                    <ul>
                        <li><strong>Dirección:</strong> Tejar el Guarco,Cartago, Costa Rica </li>
                        <li><strong>Fecha:</strong> {fecha} a las <strong>{hora}</strong></li>
                        <li><strong>Motivo:</strong> {servicio}</li>
                        <li><strong>Especialidad:</strong> {especialidad}</li>
                        <li><strong>Con el especialista:</strong> {especialista}</li>
                    </ul>
                    <p style='color: #555;'>Si necesitas contactarse con nosotros puedes hacerlo por el siguiente medio:</p>
                    <ul>
                        <li><strong>Teléfono: 8304 0865</li>
                    </ul>
            <br />

            <p>Gracias por confiar en Thames Dental.</p>
        </body>
        </html>" 
            : $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h3 style='color: #007bff;'>Nueva cita agendada en Thames Dental</h3>
                    <p><strong>Estimado(a) {nombre},</strong></p>
                    <p>Los detalles de tu próxima cita son:</p> 
                    <ul>
                        <li><strong>Dirección:</strong> Tejar el Guarco,Cartago, Costa Rica </li>
                        <li><strong>Fecha:</strong> {fecha} a las <strong>{hora}</strong></li>
                        <li><strong>Motivo:</strong> {servicio}</li>
                        <li><strong>Especialidad:</strong> {especialidad}</li>
                        <li><strong>Con el especialista:</strong> {especialista}</li>
                    </ul>
                    <p style='color: #555;'>Si necesitas contactarse con nosotros puedes hacerlo por el siguiente medio:</p>
                    <ul>
                        <li><strong>Teléfono: 8304 0865</li>
                    </ul>
                    <p style='color: #555;'>Gracias por confiar en nosotros. Nos vemos</p>
                </body>
                </html>"
                ;

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
