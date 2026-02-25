using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Http;

namespace Inventario.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        // Configuración para que el JSON sea compatible entre MVC y API
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // Filtro de seguridad: Verifica que la sesión exista y sea Admin
        private bool EsAdministrador()
        {
            var cedula = HttpContext.Session.GetString("UsuarioCedula");
            var rol = HttpContext.Session.GetInt32("UsuarioRol");
            // Validamos que no sea nulo para evitar ArgumentNullException
            return !string.IsNullOrEmpty(cedula) && rol == 1;
        }

        [HttpGet]
        public IActionResult RegistrarEmpleado()
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarEmpleado(Usuario usuario)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            // Limpieza de datos preventiva
            if (usuario == null || string.IsNullOrEmpty(usuario.Cedula))
            {
                ViewBag.Error = "Datos de usuario inválidos.";
                return View(usuario);
            }

            usuario.Rol = 0; // Forzamos rol Empleado (0)
            usuario.Cedula = usuario.Cedula.Trim(); // Quitamos espacios para PostgreSQL

            var json = JsonConvert.SerializeObject(usuario, _jsonSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Llamamos a la API de Usuarios
                var response = await _httpClient.PostAsync("api/usuarios", content);

                if (response.IsSuccessStatusCode)
                {
                    // MEJORA 1: Mensaje de éxito que saldrá en el cuadrito verde del _Layout
                    TempData["Success"] = $"Empleado {usuario.Nombre} registrado con éxito.";
                    return RedirectToAction("Index", "Parabrisas");
                }
                else
                {
                    // MEJORA 2: El código "Chismoso" por si la API falla (ej. Cédula repetida)
                    var errorApi = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"No se pudo registrar. Error de la API: {errorApi}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error de conexión con la API: " + ex.Message;
            }

            return View(usuario);
        }
    }
}
