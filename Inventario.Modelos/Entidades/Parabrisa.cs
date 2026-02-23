using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventario.Modelos.Entidades
{
    public class Parabrisa
    {
        [Key]public int Id {  get; set; }
        public int MarcaId { get; set; }

        [ForeignKey("MarcaId")] public virtual Marca? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Anio { get; set; }
        public string? Tipo { get; set; }

        public double Precio { get; set; }

        public int CantidadStock { get; set; }

        public string? Ubicación { get; set; }
    }
}
