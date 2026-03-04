using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Http;

namespace Inventario.MVC.Controllers
{
    public class ConsultasController : Controller
    {
        private readonly HttpClient _httpClient;

        public ConsultasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        private bool UsuarioEstaAutenticado()
        {
            return HttpContext.Session.GetString("UsuarioCedula") != null;
        }

        public async Task<IActionResult> Inventario(string marca, string modelo)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Parabrisas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);

                    // CORRECCIÓN: Filtro simplificado para texto
                    if (!string.IsNullOrEmpty(marca))
                    {
                        lista = lista.Where(p => !string.IsNullOrEmpty(p.Marca) && p.Marca.Contains(marca, StringComparison.OrdinalIgnoreCase));
                    }

                    if (!string.IsNullOrEmpty(modelo))
                    {
                        lista = lista.Where(p => !string.IsNullOrEmpty(p.Modelo) && p.Modelo.Contains(modelo, StringComparison.OrdinalIgnoreCase));
                    }

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