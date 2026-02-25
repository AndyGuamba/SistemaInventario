using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Inventario.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // IMPLEMENTACIÓN 1: Correo Simple (OTP)
        public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje)
        {
            await EnviarCorreoAsync(destinatario, asunto, mensaje, null, string.Empty);
        }

        // IMPLEMENTACIÓN 2: Correo Completo (Reportes con PDF)
        public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje, byte[] archivoAdjunto, string nombreArchivo)
        {
            // Leemos tus llaves exactas del JSON
            string correoOrigen = _config["EmailSettings:SenderEmail"];
            string claveOrigen = _config["EmailSettings:SenderPassword"];
            string hostSmtp = _config["EmailSettings:SmtpServer"];
            int portSmtp = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");

            // --- 🛡️ DEFENSA 1: Protocolos de Google para la nube ---
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            // Ignoramos validaciones estrictas de certificados en Linux/Render
            ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(correoOrigen, "Seguridad - Inventario");
            mailMessage.To.Add(destinatario);
            mailMessage.Subject = asunto;
            mailMessage.Body = mensaje;
            mailMessage.IsBodyHtml = true;

            if (archivoAdjunto != null && archivoAdjunto.Length > 0)
            {
                var stream = new MemoryStream(archivoAdjunto);
                var attachment = new Attachment(stream, nombreArchivo, "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            using (var smtpClient = new SmtpClient(hostSmtp))
            {
                smtpClient.Port = portSmtp;
                smtpClient.Credentials = new NetworkCredential(correoOrigen, claveOrigen);
                smtpClient.EnableSsl = true;

                // --- 🛡️ DEFENSA 2: Anti-cuelgues en Render ---
                smtpClient.Timeout = 15000; // Si en 15 seg no sale, falla rápido y no cuelga el MVC
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}