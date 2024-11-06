using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

public class GoogleCalendarService // Cambié el nombre aquí
{
    private readonly IConfiguration _configuration;

    public GoogleCalendarService(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public async Task AgregarEventoGoogleCalendarAsync(string summary, string description, string location, DateTime startDateTime, DateTime endDateTime)
    {
        try
        {
            var clientId = _configuration["GoogleAuthSettings:ClientId"];
            var clientSecret = _configuration["GoogleAuthSettings:ClientSecret"];
            var refreshToken = _configuration["GoogleAuthSettings:RefreshToken"];
            var userEmail = _configuration["GoogleAuthSettings:UserEmail"];

            var tokenResponse = new TokenResponse { RefreshToken = refreshToken };
            var clientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret };

            var credential = new UserCredential(
                new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets
                }), userEmail, tokenResponse);

            Console.WriteLine("Refreshing token...");
            await credential.RefreshTokenAsync(CancellationToken.None);
            Console.WriteLine("Token refreshed successfully.");

            var calendarService = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Thames Dental"
            });

            var newEvent = new Event
            {
                Summary = summary,
                Description = description,
                Location = location,
                Start = new EventDateTime
                {
                    DateTimeRaw = startDateTime.ToString("o"),
                    TimeZone = "America/Costa_Rica"
                },
                End = new EventDateTime
                {
                    DateTimeRaw = endDateTime.ToString("o"),
                    TimeZone = "America/Costa_Rica"
                },

            };

            Console.WriteLine("Inserting event...");
            var createdEvent = await calendarService.Events.Insert(newEvent, "primary").ExecuteAsync();
            Console.WriteLine($"Event created: {createdEvent.HtmlLink}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding event to Google Calendar: {ex.Message}");
        }
    }



}

