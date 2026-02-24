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
            // Creamos el objeto con la cédula como identificador principal
            var loginData = new
            {
                Cedula = cedula,
                Contraseña = password,
                Rol = rol
            };

            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            // Llamamos al método Login de la API que verifica el Hash de BCrypt
            var response = await _httpClient.PostAsync("api/Usuarios/login", content);

            if (response.IsSuccessStatusCode)
            {
                // Si la API confirma que es válido, guardamos los datos en la SESIÓN
                HttpContext.Session.SetString("UsuarioCedula", cedula);
                HttpContext.Session.SetInt32("UsuarioRol", rol);

                // Redirigimos según el rol (0 = Empleado, 1 = Admin)
                if (rol == 1)
                {
                    return RedirectToAction("Index", "Marcas");
                }
                else
                {
                    // Cambié esto a 'Index' de Parabrisas o tu vista de consulta
                    return RedirectToAction("Index", "Parabrisas");
                }
            }

            // Si falla, volvemos al login con un mensaje de error
            ViewBag.Error = "Cédula o contraseña incorrectos para el rol seleccionado.";
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