using Microsoft.AspNetCore.Mvc;
using Inventario.Modelos.Entidades;
using Newtonsoft.Json;

namespace Inventario.MVC.Controllers
{
    public class ParabrisasController : Controller
    {
        private readonly HttpClient _httpClient;

        public ParabrisasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        public async Task<IActionResult> Index()
        {
            // Llamada a la API de Render: GET /api/Parabrisas
            var response = await _httpClient.GetAsync("api/Parabrisas");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);
                return View(lista);
            }
            return View(new List<Parabrisa>());
        }
    }
}