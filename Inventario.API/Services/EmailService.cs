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
            // 1. LEEMOS LA CONFIGURACIÓN (Asegúrate de que las llaves coincidan con tu JSON)
            string hostSmtp = _config["EmailSettings:Host"];
            int portSmtp = int.Parse(_config["EmailSettings:Port"] ?? "587");
            string correoOrigen = _config["EmailSettings:User"];
            string claveOrigen = _config["EmailSettings:Pass"];
            string nombreOrigen = _config["EmailSettings:FromName"];
            bool useSsl = bool.Parse(_config["EmailSettings:UseSsl"] ?? "true"); // <-- AQUÍ SE DECLARA useSsl

            // 2. CREAMOS EL MENSAJE (AQUÍ SE DECLARA mailMessage)
            var mailMessage = new MailMessage(); // <-- ESTA ES LA LÍNEA QUE TE FALTABA
            mailMessage.From = new MailAddress(correoOrigen, nombreOrigen);
            mailMessage.To.Add(destinatario);
            mailMessage.Subject = asunto;
            mailMessage.Body = mensaje;
            mailMessage.IsBodyHtml = true;

            // 3. ADJUNTAMOS EL ARCHIVO (Si existe)
            if (archivoAdjunto != null && archivoAdjunto.Length > 0)
            {
                var stream = new MemoryStream(archivoAdjunto);
                var attachment = new Attachment(stream, nombreArchivo, "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            // 4. CONFIGURAMOS EL CARTERO (SmtpClient)
            using (var smtpClient = new SmtpClient(hostSmtp))
            {
                smtpClient.Port = portSmtp;
                smtpClient.Credentials = new NetworkCredential(correoOrigen, claveOrigen);
                smtpClient.EnableSsl = useSsl; // <-- AHORA SÍ RECONOCE useSsl

                // Refuerzos para que Render no se cuelgue
                smtpClient.Timeout = 20000;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                // Protocolos de seguridad modernos para Gmail
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                // ¡POR FIN ENVIAMOS!
                await smtpClient.SendMailAsync(mailMessage); // <-- AHORA SÍ RECONOCE mailMessage
            }
        }
    }
}