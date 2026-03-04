using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventario.Modelos.Entidades
{
    public class Parabrisa
    {
        [Key]
        public int Id { get; set; }

        // 🔥 EL CAMBIO ESTRELLA: Adiós llaves foráneas. 
        // Ahora es un texto libre para escribir la marca directamente.
        public string? Marca { get; set; }

        public string? Modelo { get; set; }
        
        public string? Anio { get; set; }
        
        public string? Tipo { get; set; }

        public double Precio { get; set; }

        public int Stock { get; set; }

        public string? Ubicación { get; set; }
    }
}
