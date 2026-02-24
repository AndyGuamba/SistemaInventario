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

        // 1. MÉTODO DE LOGIN (El más importante para tu MVC)
        // Este método valida si el usuario, contraseña y rol son correctos
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Usuario loginRequest)
        {
            // Buscamos al usuario por su nombre en la DB de Render
            var usuarioDB = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Nombre == loginRequest.Nombre);

            if (usuarioDB == null)
                return Unauthorized(new { mensaje = "Usuario no encontrado" });

            // Verificamos si el Hash de la DB coincide con la clave que escribió el usuario
            bool passwordValida = BCrypt.Net.BCrypt.Verify(loginRequest.Contraseña, usuarioDB.Contraseña);

            // También validamos que el Rol (0 o 1) sea el que seleccionó en el Index del MVC
            if (!passwordValida || usuarioDB.Rol != loginRequest.Rol)
                return Unauthorized(new { mensaje = "Credenciales o Rol incorrectos" });

            // Si todo está bien, devolvemos éxito
            return Ok(new { mensaje = "¡Bienvenido!", usuario = usuarioDB.Nombre, rol = usuarioDB.Rol });
        }

        // 2. POST: api/Usuarios (REGISTRO)
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Creamos el Hash: "Hola123" -> "$2a$11$K8..."
            // Esto protege la clave incluso si alguien entra a tu PostgreSQL en Render
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        // 3. GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // 4. DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}