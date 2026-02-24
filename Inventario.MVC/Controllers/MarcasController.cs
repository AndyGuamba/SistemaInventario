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
            // Usamos el cliente configurado en Program.cs para conectar con Render
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // 1. Verificación de seguridad para proteger la gestión de marcas
        private bool EsAdministrador()
        {
            var cedula = HttpContext.Session.GetString("UsuarioCedula");
            var rol = HttpContext.Session.GetInt32("UsuarioRol");
            // Solo permite el paso si hay sesión y el rol es 1 (Admin)
            return cedula != null && rol == 1;
        }

        public async Task<IActionResult> Index()
        {
            // 2. Filtro de acceso: Si no es admin, lo mandamos al inicio o login
            if (!EsAdministrador()) 
            {
                return RedirectToAction("Index", "Acceso"); 
            }

            try 
            {
                // Llamada a la API de Render: GET /api/Marcas
                var response = await _httpClient.GetAsync("api/Marcas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var marcas = JsonConvert.DeserializeObject<IEnumerable<Marca>>(content);
                    return View(marcas);
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo conectar con la API de marcas.";
            }

            return View(new List<Marca>());
        }

        // Aquí puedes agregar los métodos para Crear, Editar y Eliminar marcas siguiendo la misma lógica.
    }
}