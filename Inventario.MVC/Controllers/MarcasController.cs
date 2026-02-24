using Microsoft.AspNetCore.Mvc;
using Inventario.Modelos.Entidades;
using Newtonsoft.Json;

namespace Inventario.MVC.Controllers
{
    public class MarcasController : Controller
    {
        private readonly HttpClient _httpClient;

        public MarcasController(IHttpClientFactory httpClientFactory)
        {
            // Usamos el cliente configurado en Program.cs
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        public async Task<IActionResult> Index()
        {
            // Llamada a la API de Render: GET /api/Marcas
            var response = await _httpClient.GetAsync("api/Marcas");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var marcas = JsonConvert.DeserializeObject<IEnumerable<Marca>>(content);
                return View(marcas);
            }
            return View(new List<Marca>());
        }
    }
}   