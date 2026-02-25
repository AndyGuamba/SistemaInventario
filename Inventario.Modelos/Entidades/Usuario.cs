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
        [Key] public string? Cedula {  get; set; }
        public string? Nombre { get; set; }

        public string? Correo { get; set; }

        public string? Contraseña { get; set; }

        public RolUsuario Rol { get; set; }
        // Almacena el código de 6 dígitos enviado al correo
        public string? CodigoVerificacion { get; set; }

        // Define el tiempo de vida del código (ej. 10 minutos)
        public DateTime? FechaExpiracionCodigo { get; set; }
    }
}
