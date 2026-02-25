using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Inventario.Modelos.Entidades;
using Inventario.Modelos.DTOs;
using System.Text;

namespace Inventario.MVC.Controllers // <-- Fíjate que dice MVC
{
    // Al heredar de 'Controller', TempData y View() funcionarán perfecto
    public class OTPController : Controller
    {
        private readonly HttpClient _httpClient;

        public OTPController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("InventarioApi");
        }

        [HttpGet]
        public IActionResult Validar()
        {
            if (TempData["CedulaAdmin"] == null) return RedirectToAction("Index", "Acceso");
            TempData.Keep("CedulaAdmin");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirmar(string codigo)
        {
            var cedula = TempData["CedulaAdmin"]?.ToString();
            if (string.IsNullOrEmpty(cedula)) return RedirectToAction("Index", "Acceso");

            var loginDto = new Login2FA { Cedula = cedula.Trim(), Codigo = codigo };
            var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/usuarios/ValidarCodigo", content);

            if (response.IsSuccessStatusCode)
            {
                var usuarioJson = await response.Content.ReadAsStringAsync();
                var usuario = JsonConvert.DeserializeObject<Usuario>(usuarioJson);

                if (usuario != null && !string.IsNullOrEmpty(usuario.Cedula))
                {
                    HttpContext.Session.SetString("UsuarioCedula", usuario.Cedula);
                    HttpContext.Session.SetInt32("UsuarioRol", (int)usuario.Rol);
                    HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "Admin");
                    return RedirectToAction("Index", "Parabrisas");
                }
            }

            ViewBag.Error = "Código incorrecto o expirado.";
            TempData["CedulaAdmin"] = cedula;
            return View("Validar");
        }
    }
}