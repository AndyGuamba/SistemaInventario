using Microsoft.AspNetCore.Mvc;
using Inventario.Modelos.Enums; // Asegúrate de apuntar a la carpeta donde está tu Enum

namespace Inventario.MVC.Controllers
{
    public class AccesoController : Controller
    {
        // Página principal de selección de perfil
        public IActionResult Index()
        {
            return View();
        }

        // El parámetro 'rol' ahora puede recibirse como int (0 o 1)
        public IActionResult Login(int rol)
        {
            // Validamos que el rol sea un valor válido del Enum
            ViewBag.RolValor = rol;
            ViewBag.RolNombre = (rol == 1) ? "Administrador" : "Empleado";
            return View();
        }

        [HttpPost]
        public IActionResult Validar(string usuario, string password, int rol)
        {
            // Usamos la lógica de tus Enums: 1 para Admin, 0 para Empleado
            if (rol == 1)
            {
                // Lógica para Administrador
                return RedirectToAction("Index", "Marcas");
            }
            else if (rol == 0)
            {
                // Lógica para Empleado
                return RedirectToAction("Index", "Parabrisas");
            }

            return RedirectToAction("Index");
        }
    }
}