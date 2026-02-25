using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventario.API.Data;
using Inventario.API.Reportes;
using Inventario.API.Services;

namespace Inventario.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly InventarioApiContext _context;
        private readonly IEmailService _emailService;

        public ReportesController(InventarioApiContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("descargar")]
        public async Task<IActionResult> DescargarPdf()
        {
            // Incluimos la marca para que el reporte no salga con campos vacíos
            var lista = await _context.Parabrisas.Include(p => p.Marca).ToListAsync();
            var pdfBytes = GeneradorPdfInventario.Generar(lista);

            return File(pdfBytes, "application/pdf", "Inventario_Parabrisas.pdf");
        }

        [HttpPost("enviar/{cedula}")]
        public async Task<IActionResult> EnviarPdfPorCorreo(string cedula)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Cedula.Trim() == cedula.Trim());

            if (usuario == null || string.IsNullOrEmpty(usuario.Correo))
                return NotFound("Usuario o correo electrónico no encontrado.");

            var lista = await _context.Parabrisas.Include(p => p.Marca).ToListAsync();
            var pdfBytes = GeneradorPdfInventario.Generar(lista);

            try
            {
                await _emailService.EnviarCorreoAsync(
                    usuario.Correo,
                    "Reporte de Inventario - Parabrisas",
                    $"Hola {usuario.Nombre}, adjunto encontrarás el reporte actual de nuestro inventario.",
                    pdfBytes,
                    "Reporte_Inventario.pdf"
                );

                return Ok(new { mensaje = "Correo enviado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al enviar el correo: {ex.Message}");
            }
        }
    }
}