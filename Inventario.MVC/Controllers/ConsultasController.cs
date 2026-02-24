using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Http; // Necesario para las sesiones

namespace Inventario.MVC.Controllers
{
    public class ConsultasController : Controller
    {
        private readonly HttpClient _httpClient;

        public ConsultasController(IHttpClientFactory httpClientFactory)
        {
            // Usamos el cliente configurado en Program.cs
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // 1. FILTRO DE SEGURIDAD: Evita que entren sin loguearse
        private bool UsuarioEstaAutenticado()
        {
            return HttpContext.Session.GetString("UsuarioCedula") != null;
        }

        // Vista de búsqueda rápida
        public async Task<IActionResult> Inventario()
        {
            // Si no hay sesión, mandarlo al inicio
            if (!UsuarioEstaAutenticado())
            {
                return RedirectToAction("Index", "Acceso");
            }

            try
            {
                // Llamamos al controlador de Parabrisas que YA existe en la API
                var response = await _httpClient.GetAsync("api/Parabrisas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);

                    // Si 'lista' es nulo, enviamos una lista vacía para que la vista no explote
                    return View(lista ?? new List<Parabrisa>());
                }
            }
            catch (Exception)
            {
                // Si la conexión falla, evitamos la pantalla roja devolviendo una lista vacía
                ViewBag.Error = "Error de conexión con la base de datos.";
            }

            return View(new List<Parabrisa>());
        }
    }
}