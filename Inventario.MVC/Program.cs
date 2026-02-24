namespace Inventario.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configuración de Servicios
            builder.Services.AddControllersWithViews();

            // Configuración del HttpClient para conectar con la API en Render
            var apiBaseUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl");
            builder.Services.AddHttpClient("InventarioApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

            // AGREGADO: Configuración de Sesiones para manejar el login
            builder.Services.AddDistributedMemoryCache(); // Requerido para almacenar sesiones en memoria
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // La sesión expira tras 30 min de inactividad
                options.Cookie.HttpOnly = true; // Protege la cookie contra ataques XSS
                options.Cookie.IsEssential = true; // Permite que la cookie funcione aunque el usuario no acepte cookies de rastreo
            });

            var app = builder.Build();

            // 2. Configuración del Pipeline de Solicitudes (Middleware)
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // AGREGADO: Habilitar el uso de sesiones
            // Importante: Debe ir DESPUÉS de UseRouting y ANTES de UseAuthorization
            app.UseSession();

            app.UseAuthorization();

            // CORREGIDO: Ruta por defecto para que inicie directamente en el Login
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Acceso}/{action=Index}/{id?}");

            app.Run();
        }
    }
}