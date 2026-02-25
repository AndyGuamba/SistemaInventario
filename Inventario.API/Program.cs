using Inventario.API.Data;
using Inventario.API.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Infrastructure;

namespace Inventario.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var builder = WebApplication.CreateBuilder(args);

            // 1. AGREGAR POLÍTICA DE CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            // CONFIGURACIÓN DE CONTROLADORES
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<InventarioApiContext>(options =>
                options.UseNpgsql(connectionString));

            // Registro de servicios
            builder.Services.AddScoped<IEmailService, EmailService>();

            var app = builder.Build();

            // 2. USAR CORS (Debe ir antes de Authorization)
            app.UseCors("AllowAll");

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventario API V1");
                c.RoutePrefix = "swagger";
            });

            // Importante para despliegues en la nube
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}