using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Http;

namespace Inventario.MVC.Controllers
{
    public class ParabrisasController : Controller
    {
        private readonly HttpClient _httpClient;

        public ParabrisasController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // 1. Filtro de Seguridad
        private bool EsAdministrador()
        {
            var cedula = HttpContext.Session.GetString("UsuarioCedula");
            var rol = HttpContext.Session.GetInt32("UsuarioRol");
            return cedula != null && rol == 1; // 1 = Administrador
        }

        // GET: Lista de Parabrisas
        public async Task<IActionResult> Index()
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            var response = await _httpClient.GetAsync("api/Parabrisas");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var lista = JsonConvert.DeserializeObject<IEnumerable<Parabrisa>>(content);
                return View(lista ?? new List<Parabrisa>());
            }
            return View(new List<Parabrisa>());
        }

        // GET: Vista para Agregar
        public IActionResult Crear()
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            // LIMPIEZA: Ya no necesitamos cargar marcas aquí
            return View();
        }

        // POST: Guardar Nuevo Parabrisas
        [HttpPost]
        public async Task<IActionResult> Crear(Parabrisa nuevoParabrisa)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            if (!ModelState.IsValid)
            {
                return View(nuevoParabrisa);
            }

            var json = JsonConvert.SerializeObject(nuevoParabrisa);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/parabrisas", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Producto agregado correctamente al inventario.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = "No se pudo guardar el producto en la API.";
            return View(nuevoParabrisa);
        }

        // GET: Vista para Editar
        public async Task<IActionResult> Editar(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            var response = await _httpClient.GetAsync($"api/Parabrisas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parabrisa = JsonConvert.DeserializeObject<Parabrisa>(content);
                return View(parabrisa);
            }
            return RedirectToAction(nameof(Index));
        }

        // =======================================================
        // GENERAR REPORTES: Descargar o Enviar por Correo
        // =======================================================
        [HttpGet]
        public async Task<IActionResult> ExportarPdf(bool enviarCorreo)
        {
            var cedula = HttpContext.Session.GetString("UsuarioCedula");
            if (string.IsNullOrEmpty(cedula))
            {
                return RedirectToAction("Index", "Acceso");
            }

            try
            {
                if (!enviarCorreo)
                {
                    var response = await _httpClient.GetAsync("api/reportes/descargar");

                    if (response.IsSuccessStatusCode)
                    {
                        var fileBytes = await response.Content.ReadAsByteArrayAsync();
                        return File(fileBytes, "application/pdf", "Reporte_Inventario.pdf");
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo generar el documento PDF en este momento.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    var response = await _httpClient.PostAsync($"api/reportes/enviar/{cedula}", null);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "¡El reporte ha sido enviado exitosamente a su correo!";
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Hubo un problema al enviar el correo: {errorMsg}";
                    }
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión con la API: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Actualizar cambios en la Base de Datos
        [HttpPost]
        public async Task<IActionResult> Editar(int id, Parabrisa parabrisa)
        {
            var json = JsonConvert.SerializeObject(parabrisa);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Parabrisas/{id}", content);

            if (response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            return View(parabrisa);
        }

        // GET: Eliminar de la Base de Datos
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            await _httpClient.DeleteAsync($"api/Parabrisas/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}