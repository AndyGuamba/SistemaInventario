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
            // Leemos la configuración del nuevo JSON
            string correoOrigen = _config["EmailSettings:SenderEmail"];
            string claveOrigen = _config["EmailSettings:SenderPassword"];
            string hostSmtp = _config["EmailSettings:SmtpServer"];
            int portSmtp = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
            bool useSsl = bool.Parse(_config["EmailSettings:UseSsl"] ?? "true");

            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(correoOrigen, "Seguridad - Inventario");
            mailMessage.To.Add(destinatario);
            mailMessage.Subject = asunto;
            mailMessage.Body = mensaje;
            mailMessage.IsBodyHtml = true;

            if (archivoAdjunto != null && archivoAdjunto.Length > 0)
            {
                mailMessage.Attachments.Add(new Attachment(new MemoryStream(archivoAdjunto), nombreArchivo));
            }

            using (var smtpClient = new SmtpClient(hostSmtp))
            {
                smtpClient.Port = portSmtp;
                smtpClient.Credentials = new NetworkCredential(correoOrigen, claveOrigen);

                // Usamos la variable del JSON para activar el SSL
                smtpClient.EnableSsl = useSsl;

                // --- REFUERZO PARA RENDER ---
                smtpClient.Timeout = 15000; // 15 segundos máximo
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                // Esta línea es el equivalente al "SSL Mode=Require" pero para correos
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                // Saltamos la validación local de certificados por si Render tiene problemas de confianza con Gmail
                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}