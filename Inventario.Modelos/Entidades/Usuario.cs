using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Inventario.Modelos.Enums.EnumsInventario;

namespace Inventario.Modelos.Entidades
{
    public class Usuario
    {
        [Key] public int Id {  get; set; }
        public string? Nombre { get; set; }

        public string? Correo { get; set; }

        public string? Contraseña { get; set; }

        public RolUsuario Rol { get; set; }
    }
}
