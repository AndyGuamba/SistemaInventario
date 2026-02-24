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
            // Usamos el cliente configurado en Program.cs para conectar con Render
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // 1. Filtro de seguridad: Solo permite el acceso si es Administrador (Rol 1)
        private bool EsAdministrador()
        {
            var cedula = HttpContext.Session.GetString("UsuarioCedula");
            var rol = HttpContext.Session.GetInt32("UsuarioRol");
            return cedula != null && rol == 1; // 1 = Admin según tu Enum
        }

        public async Task<IActionResult> Index()
        {
            // 2. Validación de acceso: Si no es admin, redirigir al Login
            if (!EsAdministrador())
            {
                return RedirectToAction("Index", "Acceso");
            }

            try
            {
                // Llamada a la API de Render: GET /api/Parabrisas
                var response = await _httpClient.GetAsync("api/Parabrisas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);
                    return View(lista);
                }
            }
            catch (Exception ex)
            {
                // Captura errores de conexión con la API en la nube
                ViewBag.Error = "Error de conexión con el servidor: " + ex.Message;
            }

            return View(new List<Parabrisa>());
        }

        // Aquí irían tus métodos Crear, Editar y Eliminar protegidos también con EsAdministrador()
    }
}