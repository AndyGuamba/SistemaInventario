namespace Inventario.API.Data;
using Inventario.Modelos.Entidades;
using Microsoft.EntityFrameworkCore;

public class InventarioApiContext : DbContext
{
    // El constructor recibe las configuraciones (como la cadena de conexión)
    public InventarioApiContext(DbContextOptions<InventarioApiContext> options)
        : base(options)
    {
    }

    // Aquí defines tus tablas (DbSets)
    public DbSet<Parabrisa> Parabrisas { get; set; }
    public DbSet<Marca> Autos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
}

