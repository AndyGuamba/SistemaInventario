using Microsoft.AspNetCore.Mvc;
using Inventario.Modelos.Entidades;
using Newtonsoft.Json;
using System.Text; // ¡Importante para empaquetar el JSON!

namespace Inventario.MVC.Controllers
{
    public class MarcasController : Controller
    {
        private readonly HttpClient _httpClient;

        public MarcasController(IHttpClientFactory httpClientFactory)
        {
            // Usamos el cliente configurado en Program.cs para conectar con Render/Local
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // 1. Verificación de seguridad para proteger la gestión de marcas
        private bool EsAdministrador()
        {
            var cedula = HttpContext.Session.GetString("UsuarioCedula");
            var rol = HttpContext.Session.GetInt32("UsuarioRol");
            // Solo permite el paso si hay sesión y el rol es 1 (Admin)
            return !string.IsNullOrEmpty(cedula) && rol == 1;
        }

        // GET: Marcas/Index
        public async Task<IActionResult> Index()
        {
            // 2. Filtro de acceso
            if (!EsAdministrador())
            {
                return RedirectToAction("Index", "Acceso");
            }

            try
            {
                // Llamada a la API: GET /api/Marcas
                var response = await _httpClient.GetAsync("api/Marcas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var marcas = JsonConvert.DeserializeObject<IEnumerable<Marca>>(content);
                    return View(marcas);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"No se pudo conectar con la API de marcas: {ex.Message}";
            }

            return View(new List<Marca>());
        }

        // GET: Marcas/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            // Solo permitimos si es Admin
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            return View(); // Esto busca el archivo Views/Marcas/Crear.cshtml
        }

        // =======================================================
        // EL MÉTODO QUE FALTABA: Recibe los datos del formulario
        // =======================================================
        [HttpPost]
        public async Task<IActionResult> Crear(Marca nuevaMarca)
        {
            // Seguridad ante todo
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            // Si el formulario llega con errores (ej. vacío)
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Por favor, complete todos los campos requeridos.";
                return View(nuevaMarca);
            }

            // TRUCO: Forzamos el ID a 0 para que la BD lo autogenere
            nuevaMarca.Id = 0;

            // Empaquetamos la marca en JSON
            var json = JsonConvert.SerializeObject(nuevaMarca);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Disparamos la petición POST a la API
                var response = await _httpClient.PostAsync("api/marcas", content);

                if (response.IsSuccessStatusCode)
                {
                    // ¡Éxito! Mostramos mensaje verde en el Layout y volvemos a la lista
                    TempData["Success"] = "¡Marca registrada exitosamente!";
                    return RedirectToAction("Index");
                }
                else
                {
                    // Si la API lo rechaza, atrapamos el porqué
                    var errorApi = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"La API rechazó la solicitud: {errorApi}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"MVC no pudo contactar a la API: {ex.Message}";
            }

            // Si algo falla, recargamos la vista con el nombre que el usuario ya había escrito
            return View(nuevaMarca);
        }
    }
}