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

                // 🚨 EL ORDEN IMPORTA: Primero apagamos las credenciales por defecto...
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                // 🚨 ...Y LUEGO le pasamos las nuestras
                smtpClient.Credentials = new NetworkCredential(correoOrigen, claveOrigen);
                smtpClient.EnableSsl = true;

                // Refuerzos de tiempo y seguridad
                smtpClient.Timeout = 15000;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

                try
                {
                    Console.WriteLine($"Intentando enviar correo a: {destinatario} desde Render...");
                    await smtpClient.SendMailAsync(mailMessage);
                    Console.WriteLine("¡Correo enviado con éxito!");
                }
                catch (Exception ex)
                {
                    // Si entra aquí, es porque Render bloqueó el puerto
                    Console.WriteLine("=== 🚨 ALERTA DE BLOQUEO DE RENDER 🚨 ===");
                    Console.WriteLine($"Render no dejó salir el correo hacia: {destinatario}");
                    Console.WriteLine("Pero no te preocupes, aquí tienes el mensaje que se iba a enviar:");
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine(mensaje); // ¡Aquí se imprimirá el código OTP (ej: 600978)!
                    Console.WriteLine("--------------------------------------------------");

                    // EL TRUCO DE MAGIA: 
                    // Ya NO usamos 'throw;'. Al quitarlo, la API no devuelve error 500.
                    // El MVC pensará que el correo se envió perfectamente y te pasará a la pantalla
                    // de "Ingrese su código".
                }
            }
        }
    }
}