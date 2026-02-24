using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Inventario.Modelos.Entidades;

namespace Inventario.MVC.Controllers
{
    public class AccesoController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccesoController(IHttpClientFactory httpClientFactory)
        {
            // Usamos el cliente configurado para conectar con la API en Render
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        // 1. Selección de Perfil (Admin o Empleado)
        public IActionResult Index()
        {
            return View();
        }

        // 2. Muestra el formulario de Login
        public IActionResult Login(int rol)
        {
            ViewBag.RolValor = rol;
            ViewBag.RolNombre = (rol == 1) ? "Administrador" : "Empleado";
            return View();
        }

        // 3. VALIDACIÓN REAL CON LA API
        [HttpPost]
        public async Task<IActionResult> Validar(string cedula, string password, int rol)
        {
            var loginData = new
            {
                Cedula = cedula, // Usamos la nueva Llave Primaria
                Contraseña = password,
                Rol = rol
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            try
            {
                // Llamamos al método Login de la API que verifica el Hash de BCrypt
                var response = await _httpClient.PostAsync("api/Usuarios/login", content);

                if (response.IsSuccessStatusCode)
                {
                    // Guardamos los datos en la SESIÓN para el Layout
                    HttpContext.Session.SetString("UsuarioCedula", cedula);
                    HttpContext.Session.SetInt32("UsuarioRol", rol);

                    // REDIRECCIÓN POR ROL CORREGIDA
                    if (rol == 1)
                    {
                        return RedirectToAction("Index", "Marcas");
                    }
                    else
                    {
                        // El empleado va directo a su vista de solo lectura
                        return RedirectToAction("Inventario", "Consultas");
                    }
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo conectar con el servidor. Intente más tarde.";
            }

            // Si falla, volvemos al login con el mensaje correspondiente
            ViewBag.Error = ViewBag.Error ?? "Cédula o contraseña incorrectos.";
            ViewBag.RolValor = rol;
            ViewBag.RolNombre = (rol == 1) ? "Administrador" : "Empleado";
            return View("Login");
        }

        // 4. Cerrar Sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Borra los datos de la sesión
            return RedirectToAction("Index");
        }
    }
}