using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Inventario.Modelos.Entidades;
using Microsoft.AspNetCore.Http;

namespace Inventario.MVC.Controllers
{
    public class AccesoController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccesoController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Login(int rol)
        {
            ViewBag.RolValor = rol;
            ViewBag.RolNombre = (rol == 1) ? "Administrador" : "Empleado";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string cedula, string contraseña, int rol)
        {
            if (string.IsNullOrEmpty(cedula) || string.IsNullOrEmpty(contraseña))
            {
                ViewBag.Error = "Por favor, complete todos los campos.";
                ViewBag.RolValor = rol;
                ViewBag.RolNombre = (rol == 1) ? "Administrador" : "Empleado";
                return View();
            }

            var loginData = new { Cedula = cedula.Trim(), Contraseña = contraseña, Rol = rol };
            var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/usuarios/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var usuarioJson = await response.Content.ReadAsStringAsync();
                    dynamic apiResponse = JsonConvert.DeserializeObject(usuarioJson);

                    if (apiResponse != null)
                    {
                        string nombreApi = apiResponse.usuario?.ToString() ?? "Empleado";
                        int rolApi = (int?)apiResponse.rol ?? rol;
                        string cedulaApi = apiResponse.cedula?.ToString() ?? cedula.Trim();

                        // --- CASO 1: ADMINISTRADOR ---
                        if (rol == 1 && rolApi == 1)
                        {
                            var otpRequest = new { Cedula = cedulaApi };
                            var otpContent = new StringContent(JsonConvert.SerializeObject(otpRequest), Encoding.UTF8, "application/json");

                            var otpRes = await _httpClient.PostAsync("api/usuarios/GenerarOTP", otpContent);

                            if (otpRes.IsSuccessStatusCode)
                            {
                                TempData["CedulaAdmin"] = cedulaApi;
                                return RedirectToAction("Validar", "OTP");
                            }
                            else
                            {
                                // AQUÍ ESTÁ LA MAGIA ARREGLADA
                                var errorenCorreo = await otpRes.Content.ReadAsStringAsync();

                                if (string.IsNullOrWhiteSpace(errorenCorreo))
                                {
                                    errorenCorreo = $"Error HTTP {(int)otpRes.StatusCode} ({otpRes.StatusCode}). Revisa que la API esté corriendo con los últimos cambios.";
                                }

                                ViewBag.Error = $"AVISO DE SEGURIDAD: {errorenCorreo}";
                            }
                        }
                        // --- CASO 2: EMPLEADO ---
                        else if (rol == 0 && rolApi == 0)
                        {
                            HttpContext.Session.SetString("UsuarioCedula", cedulaApi);
                            HttpContext.Session.SetInt32("UsuarioRol", rolApi);
                            HttpContext.Session.SetString("UsuarioNombre", nombreApi);

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewBag.Error = "El rol seleccionado no coincide con sus credenciales.";
                        }
                    }
                }
                else
                {
                    ViewBag.Error = "Cédula o contraseña incorrectas.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"MVC NO PUDO CONECTAR: {ex.Message}";
            }

            ViewBag.RolValor = rol;
            ViewBag.RolNombre = (rol == 1) ? "Administrador" : "Empleado";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}