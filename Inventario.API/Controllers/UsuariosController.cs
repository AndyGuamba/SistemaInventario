using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventario.API.Data;
using Inventario.Modelos.Entidades;
using Inventario.Modelos.DTOs;
using Inventario.API.Services;

namespace Inventario.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly InventarioApiContext _context;
        private readonly IEmailService _emailService;

        public UsuariosController(InventarioApiContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // LOGIN PASO 1: Valida credenciales iniciales
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Usuario loginRequest)
        {
            var usuarioDB = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Cedula.Trim() == loginRequest.Cedula.Trim());

            if (usuarioDB == null)
                return Unauthorized(new { mensaje = "Cédula no encontrada" });

            bool passwordValida = BCrypt.Net.BCrypt.Verify(loginRequest.Contraseña, usuarioDB.Contraseña);

            if (!passwordValida || (int)usuarioDB.Rol != (int)loginRequest.Rol)
                return Unauthorized(new { mensaje = "Credenciales o Rol incorrectos" });

            return Ok(usuarioDB);
        }

        // LOGIN PASO 2: Generar y enviar código (Solo para Administradores)
        // CORREGIDO: Ahora usa GenerarOtpDTO que SOLO pide la Cédula
        [HttpPost("GenerarOTP")]
        public async Task<IActionResult> GenerarOTP([FromBody] GenerarOtpDTO request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Cedula.Trim() == request.Cedula.Trim() && (int)u.Rol == 1);

            if (usuario == null)
                return NotFound("Administrador no encontrado en la base de datos.");

            // Generamos código de 6 dígitos
            string codigoGenerado = new Random().Next(100000, 999999).ToString();
            usuario.CodigoVerificacion = codigoGenerado;
            usuario.FechaExpiracionCodigo = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            try
            {
                await _emailService.EnviarCorreoAsync(usuario.Correo, "Código de Seguridad", $"Tu código de acceso es: {codigoGenerado}");
                return Ok(new { mensaje = "Código enviado exitosamente" });
            }
            catch (Exception ex)
            {
                // EL SALVAVIDAS
                return StatusCode(500, $"Código generado: [{codigoGenerado}]. Pero el correo falló: {ex.Message}");
            }
        }

        // LOGIN PASO 3: Validar el código ingresado en la pantalla
        [HttpPost("ValidarCodigo")]
        public async Task<IActionResult> ValidarCodigo([FromBody] Login2FA request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Cedula.Trim() == request.Cedula.Trim());

            if (usuario == null || usuario.CodigoVerificacion != request.Codigo)
                return BadRequest("Código incorrecto.");

            if (usuario.FechaExpiracionCodigo < DateTime.UtcNow)
                return BadRequest("El código ha expirado.");

            // Limpiamos el código por seguridad una vez usado
            usuario.CodigoVerificacion = null;
            usuario.FechaExpiracionCodigo = null;
            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

        // GET BY CEDULA: Buscar empleado
        [HttpGet("Buscar/{cedula}")]
        public async Task<ActionResult<Usuario>> GetUsuario(string cedula)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Cedula.Trim() == cedula.Trim());

            if (usuario == null) return NotFound();
            return usuario;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return Ok(usuarios);
        }
       
        [HttpPost]
        public async Task<IActionResult> RegistrarUsuario([FromBody] Usuario nuevoUsuario)
        {
            if (nuevoUsuario == null || string.IsNullOrWhiteSpace(nuevoUsuario.Cedula))
                return BadRequest("Datos del usuario incompletos.");

            // 1. Verificamos que la cédula no esté repetida
            var existeUsuario = await _context.Usuarios
                .AnyAsync(u => u.Cedula.Trim() == nuevoUsuario.Cedula.Trim());

            if (existeUsuario)
                return BadRequest("Ya existe un usuario con esta cédula.");

            // 2. Encriptamos la contraseña obligatoriamente (Vital para que el Login funcione después)
            nuevoUsuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Contraseña);

            // 3. Guardamos en la Base de Datos
            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Empleado registrado con éxito." });
        }
    }
}