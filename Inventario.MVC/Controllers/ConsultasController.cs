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
        public async Task<IActionResult> Inventario(string marca, string modelo)
        {
            try
            {
                // Llamada a la API de Render
                var response = await _httpClient.GetAsync("api/Parabrisas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);

                    // Filtramos por Marca y Modelo si el usuario escribió algo
                    if (!string.IsNullOrEmpty(marca))
                    {
                        lista = lista.Where(p => p.Marca.MarcaVehiculo.Contains(marca, StringComparison.OrdinalIgnoreCase));
                    }

                    if (!string.IsNullOrEmpty(modelo))
                    {
                        lista = lista.Where(p => p.Modelo.Contains(modelo, StringComparison.OrdinalIgnoreCase));
                    }

                    // Guardamos los términos de búsqueda para que no se borren del input al recargar
                    ViewBag.MarcaBusqueda = marca;
                    ViewBag.ModeloBusqueda = modelo;

                    return View(lista.ToList());
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Error de conexión con el servidor.";
            }

            return View(new List<Parabrisa>());
        }
    }
}