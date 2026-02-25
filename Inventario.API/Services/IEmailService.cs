namespace Inventario.API.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string destino, string asunto, string mensajeHtml);
        Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje, byte[] archivoAdjunto, string nombreArchivo );
    }
}
