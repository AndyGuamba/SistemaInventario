using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
            string apiKey = _config["EmailSettings:ApiKey"];

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            // Formato exigido por la API de Brevo
            var emailData = new
            {
                sender = new { name = "Seguridad - Inventario", email = correoOrigen },
                to = new[] { new { email = destinatario } },
                subject = asunto,
                htmlContent = mensaje,
                attachment = archivoAdjunto != null && archivoAdjunto.Length > 0
                    ? new[] { new { content = Convert.ToBase64String(archivoAdjunto), name = nombreArchivo } }
                    : null
            };

            var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var jsonContent = new StringContent(JsonSerializer.Serialize(emailData, jsonOptions), Encoding.UTF8, "application/json");

            // Envío por puerto seguro HTTPS (443)
            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                Console.WriteLine("=== ERROR DE BREVO ===");
                Console.WriteLine(errorDetails);
                throw new Exception("Fallo al enviar correo por la API de Brevo.");
            }
        }
    }
}