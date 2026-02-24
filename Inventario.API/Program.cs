using Microsoft.EntityFrameworkCore;
using Inventario.API.Data;
// Importamos NewtonsoftJson para manejar los ciclos de referencia
using Newtonsoft.Json;

namespace Inventario.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. CONFIGURACIÓN DE CONTROLADORES CON NEWTONSOFTJSON
            // Esto soluciona el Error 500 al evitar que el JSON entre en bucles infinitos
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 2. CADENA DE CONEXIÓN A POSTGRESQL (Render/Local)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<InventarioApiContext>(options =>
                options.UseNpgsql(connectionString));

            var app = builder.Build();

            // 3. CONFIGURACIÓN DE SWAGGER
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventario API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}