using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventario.API.Data;
using Inventario.Modelos.Entidades;
namespace Inventario.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly InventarioApiContext _context;

        public UsuariosController(InventarioApiContext context)
        {
            _context = context;
        }

        // 1. MÉTODO DE LOGIN (Identificación por Cédula)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Usuario loginRequest)
        {
            // Ahora buscamos específicamente por el campo Cedula en PostgreSQL
            var usuarioDB = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Cedula == loginRequest.Cedula);

            if (usuarioDB == null)
                return Unauthorized(new { mensaje = "Cédula no encontrada" });

            // Verificamos si la contraseña coincide con el hash
            bool passwordValida = BCrypt.Net.BCrypt.Verify(loginRequest.Contraseña, usuarioDB.Contraseña);

            // Validamos que el Rol (0 o 1) sea el correcto
            if (!passwordValida || usuarioDB.Rol != loginRequest.Rol)
                return Unauthorized(new { mensaje = "Credenciales o Rol incorrectos" });

            return Ok(new { mensaje = "¡Bienvenido!", usuario = usuarioDB.Nombre, rol = usuarioDB.Rol });
        }

        // 2. GET: api/Usuarios (Listado completo)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // 3. GET: api/Usuarios/1752790475
        [HttpGet("{cedula}")]
        public async Task<ActionResult<Usuario>> GetUsuario(string cedula)
        {
            // Buscamos por el string de la cédula en lugar de un int Id
            var usuario = await _context.Usuarios.FindAsync(cedula);

            if (usuario == null) return NotFound();

            return usuario;
        }

        // 4. POST: api/Usuarios (REGISTRO)
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Protegemos la contraseña con BCrypt antes de guardar
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // El CreatedAtAction ahora apunta a la ruta con la cédula
            return CreatedAtAction("GetUsuario", new { cedula = usuario.Cedula }, usuario);
        }

        // 5. DELETE: api/Usuarios/1752790475
        [HttpDelete("{cedula}")]
        public async Task<IActionResult> DeleteUsuario(string cedula)
        {
            var usuario = await _context.Usuarios.FindAsync(cedula);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(string cedula)
        {
            return _context.Usuarios.Any(e => e.Cedula == cedula);
        }
    }
}
