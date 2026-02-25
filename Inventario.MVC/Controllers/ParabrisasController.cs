using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using Inventario.Modelos.Entidades;

namespace Inventario.MVC.Controllers
{
    public class ParabrisasController : Controller
    {
        private readonly HttpClient _httpClient;

        public ParabrisasController(IHttpClientFactory httpClientFactory)
        {
            // Conexión con la API configurada en Program.cs
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
        public async Task<IActionResult> Crear()
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            // CORREGIDO: Usamos el nombre consistente
            await CargarMarcasEnViewBag();
            return View();
        }

        // POST: Guardar Nuevo Parabrisas
        [HttpPost]
        public async Task<IActionResult> Crear(Parabrisa nuevoParabrisa)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            if (!ModelState.IsValid)
            {
                await CargarMarcasEnViewBag();
                return View(nuevoParabrisa);
            }

            // Serializamos el objeto Parabrisa (singular como tu modelo)
            var json = JsonConvert.SerializeObject(nuevoParabrisa);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Enviamos a la API
            var response = await _httpClient.PostAsync("api/parabrisas", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Producto agregado correctamente al inventario.";
                return RedirectToAction("Index");
            }

            ViewBag.Error = "No se pudo guardar el producto en la API.";
            await CargarMarcasEnViewBag();
            return View(nuevoParabrisa);
        }

        // GET: Vista para Editar (Permite rebajar stock)
        public async Task<IActionResult> Editar(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            var response = await _httpClient.GetAsync($"api/Parabrisas/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var parabrisa = JsonConvert.DeserializeObject<Parabrisa>(content);
                await CargarMarcas();
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
            // 1. Verificamos quién es el usuario logueado para saber a qué correo enviar
            var cedula = HttpContext.Session.GetString("UsuarioCedula");
            if (string.IsNullOrEmpty(cedula))
            {
                return RedirectToAction("Index", "Acceso");
            }

            try
            {
                if (!enviarCorreo)
                {
                    // --- CASO A: DESCARGAR EL PDF ---
                    // Llamamos a la ruta GET de la API
                    var response = await _httpClient.GetAsync("api/reportes/descargar");

                    if (response.IsSuccessStatusCode)
                    {
                        var fileBytes = await response.Content.ReadAsByteArrayAsync();
                        // Devolvemos el archivo directamente al navegador para que inicie la descarga
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
                    // --- CASO B: ENVIAR POR CORREO ---
                    // Llamamos a la ruta POST de la API pasando la cédula en la URL
                    var response = await _httpClient.PostAsync($"api/reportes/enviar/{cedula}", null);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["Success"] = "¡El reporte ha sido enviado exitosamente a su correo!";
                    }
                    else
                    {
                        // Si falla, leemos el porqué (muy útil para saber si la clave del correo está mal)
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        TempData["Error"] = $"Hubo un problema al enviar el correo: {errorMsg}";
                    }
                    return RedirectToAction("Index"); // Volvemos a la pantalla donde estábamos
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

            await CargarMarcas();
            return View(parabrisa);
        }

        // GET: Eliminar de la Base de Datos
        public async Task<IActionResult> Eliminar(int id)
        {
            if (!EsAdministrador()) return RedirectToAction("Index", "Acceso");

            await _httpClient.DeleteAsync($"api/Parabrisas/{id}");
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para cargar el listado de marcas en los formularios
        private async Task CargarMarcas()
        {
            var response = await _httpClient.GetAsync("api/Marcas");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var marcas = JsonConvert.DeserializeObject<IEnumerable<Marca>>(content);
                ViewBag.Marcas = new SelectList(marcas, "Id", "MarcaVehiculo");
            }
        }
        private async Task CargarMarcasEnViewBag()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Marcas");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var marcas = JsonConvert.DeserializeObject<List<Marca>>(content);
                    // Si la lista es nula, inicializamos una vacía para evitar el crash
                    ViewBag.Marcas = marcas ?? new List<Marca>();
                }
                else
                {
                    ViewBag.Marcas = new List<Marca>();
                }
            }
            catch (Exception)
            {
                ViewBag.Marcas = new List<Marca>();
            }
        }
    }

}