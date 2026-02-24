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

namespace Inventario.MVC.Controllers
{
    public class ParabrisasController : Controller
    {
        private readonly HttpClient _httpClient;

        public ParabrisasController(IHttpClientFactory httpClientFactory)
        {
            // Usamos el cliente configurado para conectar con la API en Render
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // GET: Parabrisas
        // Muestra el catálogo completo consumiendo la API
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/Parabrisas");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);
                return View(lista);
            }
            return View(new List<Parabrisa>());
        }

        // GET: Parabrisas/Crear
        // Carga la lista de marcas para el DropDownList antes de mostrar la vista
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarMarcasEnViewBag();
            return View();
        }

        // POST: Parabrisas/Crear
        [HttpPost]
        public async Task<IActionResult> Crear(Parabrisa parabrisa)
        {
            var json = JsonConvert.SerializeObject(parabrisa);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Parabrisas", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            await CargarMarcasEnViewBag(); // Recargar marcas si hay error
            return View(parabrisa);
        }

        // GET: Parabrisas/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var response = await _httpClient.GetAsync($"api/Parabrisas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parabrisa = JsonConvert.DeserializeObject<Parabrisa>(content);

                await CargarMarcasEnViewBag();
                return View(parabrisa);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Parabrisas/Editar/5
        [HttpPost]
        public async Task<IActionResult> Editar(int id, Parabrisa parabrisa)
        {
            var json = JsonConvert.SerializeObject(parabrisa);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/Parabrisas/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            await CargarMarcasEnViewBag();
            return View(parabrisa);
        }

        // Función auxiliar para obtener las marcas de la API y pasarlas a la vista
        private async Task CargarMarcasEnViewBag()
        {
            var response = await _httpClient.GetAsync("api/Marcas");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var marcas = JsonConvert.DeserializeObject<IEnumerable<Marca>>(content);

                // Creamos la lista para el <select> de HTML
                ViewBag.Marcas = new SelectList(marcas, "Id", "NombreMarca");
            }
        }

        // GET: Parabrisas/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            await _httpClient.DeleteAsync($"api/Parabrisas/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}