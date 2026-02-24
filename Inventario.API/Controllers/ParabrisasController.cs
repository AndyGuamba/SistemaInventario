using Inventario.API.Data;
using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventario.API.Controllers
{
    [Route("api/[controller]")] // Define la URL como /api/Parabrisas
    [ApiController] // Habilita comportamientos específicos de API y visibilidad en Swagger
    public class ParabrisasController : ControllerBase
    {
        private readonly InventarioApiContext _context;

        public ParabrisasController(InventarioApiContext context)
        {
            _context = context;
        }

        // GET: api/Parabrisas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parabrisa>>> GetParabrisas()
        {
            // Incluimos la Marca para que el JSON devuelva el nombre de la marca y no solo el ID
            return await _context.Parabrisas.Include(p => p.Marca).ToListAsync();
        }

        // GET: api/Parabrisas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Parabrisa>> GetParabrisa(int id)
        {
            var parabrisa = await _context.Parabrisas.Include(p => p.Marca)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parabrisa == null) return NotFound();

            return parabrisa;
        }

        // POST: api/Parabrisas
        [HttpPost]
        public async Task<ActionResult<Parabrisa>> PostParabrisa(Parabrisa parabrisa)
        {
            _context.Parabrisas.Add(parabrisa);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetParabrisa", new { id = parabrisa.Id }, parabrisa);
        }

        // PUT: api/Parabrisas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParabrisa(int id, Parabrisa parabrisa)
        {
            if (id != parabrisa.Id) return BadRequest();

            _context.Entry(parabrisa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParabrisaExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Parabrisas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParabrisa(int id)
        {
            var parabrisa = await _context.Parabrisas.FindAsync(id);
            if (parabrisa == null) return NotFound();

            _context.Parabrisas.Remove(parabrisa);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParabrisaExists(int id)
        {
            return _context.Parabrisas.Any(e => e.Id == id);
        }
    }
}