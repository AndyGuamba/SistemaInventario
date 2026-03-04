using Inventario.API.Data;
using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventario.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            return await _context.Parabrisas.ToListAsync();
        }

        // GET: api/Parabrisas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Parabrisa>> GetParabrisa(int id)
        {
            var parabrisa = await _context.Parabrisas.FirstOrDefaultAsync(p => p.Id == id);

            if (parabrisa == null) return NotFound();

            return parabrisa;
        }

        // ====================================================================
        // POST: api/Parabrisas (AQUÍ ESTÁ LA TRAMPA AUTOMÁTICA)
        // ====================================================================
        [HttpPost]
        public async Task<ActionResult<Parabrisa>> PostParabrisa(Parabrisa parabrisa)
        {
            // 🔴 TRUCO PRO: Esta línea hace exactamente lo mismo que el punto rojo.
            // Va a pausar el Visual Studio apenas el MVC intente guardar algo.
            System.Diagnostics.Debugger.Break();

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