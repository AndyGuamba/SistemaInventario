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

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje)
        {
            await EnviarCorreoAsync(destinatario, asunto, mensaje, null, string.Empty);
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje, byte[] archivoAdjunto, string nombreArchivo)
        {
            string correoOrigen = _config["EmailSettings:SenderEmail"];
            string claveOrigen = _config["EmailSettings:SenderPassword"];
            string hostSmtp = _config["EmailSettings:SmtpServer"];
            int portSmtp = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");

            // ============================================================
            // 🛡️ REFUERZO DE SEGURIDAD SSL/TLS (Agrega esto)
            // ============================================================
            // Forzamos el uso de TLS 1.2, que es lo que Gmail exige
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Esta línea le dice a la API que confíe en el certificado del servidor de correo
            // Evita que Render bloquee la conexión por temas de certificados SSL
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
                smtpClient.EnableSsl = true; // Aquí activamos el SSL/TLS

                // Tiempo de espera para no colgar el MVC (15 segundos)
                smtpClient.Timeout = 15000;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}