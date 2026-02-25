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
            // Reutiliza el método sobrecargado enviando null en el adjunto
            await EnviarCorreoAsync(destinatario, asunto, mensaje, null, string.Empty);
        }

        // IMPLEMENTACIÓN 2: Correo Completo (Reportes con PDF)
        public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje, byte[] archivoAdjunto, string nombreArchivo)
        {
            // Leemos las llaves exactas de tu appsettings.json o Variables de Entorno en Render
            string correoOrigen = _config["EmailSettings:SenderEmail"];
            string claveOrigen = _config["EmailSettings:SenderPassword"];
            string hostSmtp = _config["EmailSettings:SmtpServer"];
            int portSmtp = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");

            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(correoOrigen, "Seguridad - Inventario");
            mailMessage.To.Add(destinatario);
            mailMessage.Subject = asunto;
            mailMessage.Body = mensaje;
            mailMessage.IsBodyHtml = true;

            // Adjuntar archivo si existe (para reportes de inventario)
            if (archivoAdjunto != null && archivoAdjunto.Length > 0)
            {
                var stream = new MemoryStream(archivoAdjunto);
                var attachment = new Attachment(stream, nombreArchivo, "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            using (var smtpClient = new SmtpClient(hostSmtp))
            {
                // CONFIGURACIÓN DE SEGURIDAD PARA RENDER:
                smtpClient.Port = portSmtp; // Usualmente 587
                smtpClient.Credentials = new NetworkCredential(correoOrigen, claveOrigen);
                smtpClient.EnableSsl = true;

                // 1. Timeout corto (15 seg) para evitar que el MVC se quede cargando 100 segundos
                smtpClient.Timeout = 15000;

                // 2. Obligatorio para servidores en la nube como Render
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                // 3. Forzamos protocolos de seguridad modernos requeridos por Gmail
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

                // Disparamos el envío
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}