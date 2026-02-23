using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventario.Modelos.Entidades
{
    public class Marca
    {
        [Key]public int Id { get; set; }
        public string? MarcaVehiculo { get; set; }

        public virtual ICollection<Parabrisa> Parabrisas { get; set; } = new List<Parabrisa>();
    }
}
