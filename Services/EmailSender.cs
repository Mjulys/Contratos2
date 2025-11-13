using System.Threading.Tasks;

namespace Contratos2.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class EmailSender : IEmailSender
    {
        // Para desenvolvimento: apenas loga o email
        // Em produção, implementar envio real (SendGrid, SMTP, etc.)
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Log para desenvolvimento
            System.Diagnostics.Debug.WriteLine($"Email para: {email}");
            System.Diagnostics.Debug.WriteLine($"Assunto: {subject}");
            System.Diagnostics.Debug.WriteLine($"Mensagem: {message}");
            
            return Task.CompletedTask;
        }
    }
}

