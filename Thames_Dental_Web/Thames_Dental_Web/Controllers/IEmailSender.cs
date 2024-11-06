public interface IEmailSender
{
    Task SendEmailAsync(string email, 
        string subject, string nombre, 
        string fecha, string hora, string especialidad, 
        string servicio, string especialista,
        bool isCancellation = false,
        bool isReschedule = false
        );
}
