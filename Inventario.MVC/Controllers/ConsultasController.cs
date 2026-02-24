using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Inventario.MVC.Controllers
{
    public class ConsultasController : Controller
    {
        private readonly HttpClient _httpClient;

        public ConsultasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // Vista de búsqueda rápida
        public async Task<IActionResult> Inventario()
        {
            var response = await _httpClient.GetAsync("api/Parabrisas");
            var content = await response.Content.ReadAsStringAsync();
            var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);
            return View(lista); // El empleado verá una tabla de "Solo Lectura"
        }
    }
}