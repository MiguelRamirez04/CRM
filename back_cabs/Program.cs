using System.Text;
using CRM.Config;
using Serilog;
using back_cabs.CRM.contexts;
using back_cabs.CRM.services;
using back_cabs.CRM.services.Auth;
using back_cabs.CRM.services.Fleet;
using back_cabs.services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using back_cabs.CRM.Middleware;
using StackExchange.Redis;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.services.shared;
using back_cabs.CRM.Repositories;
using back_cabs.CRM.Services.Shared;

var builder = WebApplication.CreateBuilder(args);

// Redis cache registration (si aún no está)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "CABS_";
});

// opcional: ConnectionMultiplexer si lo usaras directamente
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var cfg = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("RedisConnection")!, true);
    return ConnectionMultiplexer.Connect(cfg);
});

// registrar CacheService
builder.Services.AddScoped<ICacheService, CacheService>();

// Configurar Serilog temprano para capturar logs de startup
builder.Host.UseSerilog();

// Agregar configuraciones centralizadas
builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddValidationConfiguration();
builder.Services.AddMediatRConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);


// Servicios básicos de ASP.NET Core
builder.Services.AddControllers();

// Inyección de contextos de base de datos
builder.Services.AddDbContext<ReadOnlyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<WriteContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyección de repositorios (Repository Pattern)
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Shared.IVehiculoRepository, back_cabs.CRM.repositories.Shared.VehiculoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Recepcion.IEjecucionOrdenRepository, back_cabs.CRM.repositories.Recepcion.EjecucionOrdenRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Auth.IUsuarioAuthRepository, back_cabs.CRM.repositories.Auth.UsuarioAuthRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Recepcion.ICotizacionRepository, back_cabs.CRM.repositories.Recepcion.CotizacionRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Recepcion.IOrdenTrabajoRepository, back_cabs.CRM.repositories.Recepcion.OrdenTrabajoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.IClientesLegacyValidationRepository, back_cabs.CRM.repositories.ClientesLegacyValidationRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Soporte.IReparacionRepository, back_cabs.CRM.repositories.Soporte.ReparacionRepository>();

// Inyección de servicios de la aplicación
builder.Services.AddScoped<IServicioJwt, ServicioJwt>(); // ✅ Ahora usa interfaz para mejor testabilidad
builder.Services.AddScoped<UsuarioAuthService>();
builder.Services.AddScoped<VehiculosService>();
builder.Services.AddScoped<ClientesCompletosService>();
builder.Services.AddScoped<IFotosEvaluacion, FotosEvaluacionService>();
builder.Services.AddScoped<back_cabs.CRM.services.Recepcion.OrdenTrabajoService>();
builder.Services.AddScoped<back_cabs.CRM.services.Recepcion.CotizacionService>();
builder.Services.AddScoped<back_cabs.CRM.Services.Shared.GastoViaticoService>();
builder.Services.AddScoped<back_cabs.CRM.services.Recepcion.DashRecepcionService>();
builder.Services.AddScoped<back_cabs.CRM.services.Soporte.ReparacionService>();
builder.Services.AddScoped<back_cabs.CRM.services.shared.EjecucionOrdenService>();
builder.Services.AddScoped<back_cabs.CRM.services.shared.EvaluacionDetallesService>();

//Interfaces del los servicios que acabamos de realizar
// Registra el repositorio para que el servicio pueda usarlo
builder.Services.AddScoped<back_cabs.CRM.Interfaces.IDetalleEvaluacionRepository, back_cabs.CRM.Repositories.DetalleEvaluacionRepository>();
builder.Services.AddScoped<IGastoViaticoRepository, GastoViaticoRepository>();
builder.Services.AddScoped<IGastoViaticoService, GastoViaticoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Soporte.IReparacionFotoRepository, back_cabs.CRM.Repositories.Soporte.ReparacionFotoRepository>();
// Servicio de depuración para problemas de clientes legacy
builder.Services.AddScoped<back_cabs.CRM.services.ClientesLegacyValidationService>();
builder.Services.AddScoped<back_cabs.CRM.services.shared.EvaluacionService>();
builder.Services.AddScoped<back_cabs.CRM.services.shared.FotosEvaluacionService>();

// Servicios de procesamiento de imágenes y gestión de archivos
builder.Services.AddScoped<back_cabs.CRM.services.shared.ImageProcessingService>();
builder.Services.AddScoped<back_cabs.CRM.services.Soporte.ReparacionFotoService>();

// Servicio genérico de almacenamiento de archivos
builder.Services.AddScoped<back_cabs.CRM.services.Files.IFileStorageService, back_cabs.CRM.services.Files.FileStorageService>();

// Registrar la conexión a la base de datos para inyectar IDbConnection
builder.Services.AddTransient<System.Data.IDbConnection>(sp => 
    new Microsoft.Data.SqlClient.SqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Validación de tokens JWT en rutas específicas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("user", "admin"));
});

// Configuración de Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("API Status", () => HealthCheckResult.Healthy("API is up and running"))
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found."), 
        name: "Database")
    .AddCheck("Custom Health Check", () =>
    {
        // Lógica de verificación personalizada
        bool healthCheckPassed = true; // Reemplazar con lógica real
        return healthCheckPassed ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    });

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:5176", "https://localhost:5176") // Angular
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // CRÍTICO: Para cookies HttpOnly
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .WithExposedHeaders("X-CSRF-Token"); // Para CSRF protection
    });
    
    // Política más restrictiva para producción
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://your-production-domain.com")
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-CSRF-Token")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours(24)); // Cache preflight 24h
    });
});

// Configuración de cookies seguras
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AuthToken";
    options.Cookie.HttpOnly = true; // CRÍTICO: Previene acceso desde JavaScript
    options.Cookie.SecurePolicy = builder.Environment.IsProduction() 
        ? CookieSecurePolicy.Always 
        : CookieSecurePolicy.SameAsRequest; // Solo HTTPS en producción
    options.Cookie.SameSite = builder.Environment.IsProduction() 
        ? SameSiteMode.Strict 
        : SameSiteMode.Lax; // CSRF protection
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// Configuración de límite de tamaño de archivos para subida
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();

// Middleware pipeline con seguridad mejorada
app.UseGlobalErrorHandling();
app.UseSecurityHeaders();
app.UseRequestResponseLogging();

// CORS (usar política apropiada según el entorno)
var corsPolicy = app.Environment.IsProduction() ? "Production" : "SecureFrontend";
app.UseCors(corsPolicy);

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI (solo en desarrollo y staging)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API v1");
        c.RoutePrefix = "swagger";
    });
}

// Health checks
app.UseHealthChecksConfiguration();

// Controladores
app.MapControllers();

// Logging de inicio
app.Logger.LogInformation("🚀 CRM API iniciada correctamente");

app.Run();


Console.WriteLine("jajaja quien dejo un console writeline aqui");

Console.WriteLine("⣈⣽⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢘⣿⣿ \n⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢿⣿ \n⣯⡏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⡻ \n⣿⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐\n⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n ⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n                        ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⢰⢧⠀⠀⢸⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⠀⠀⠸⠀⠀⠀⠀⠀⠀⠀⠀⡇⢸⡘⡄⠀⢨⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⣼⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⢧⢸⣷⡡⠀⡀⣯⢆⠀⠀⠀⠀⠀⠀⡇⠀⠀⠀⠂⠀⣀⠀⢸⡿⠿⠀⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⠷⠘⠦⢀⡑⠇⢻⣯⣂⡀⠀⣶⠀⢠⡗⠇⠀⠀⠀⠰⠟⢀⣵⠾⠗⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⠀⠀⠀⠀⠀⠀⠀⠀⠀⠄⠀⣤⣄⠄⠀⠉⠲⢠⣍⣓⠉⠀⢹⠀⣸⡻⢡⠀⠀⢠⣶⢋⠊⠋⠀⠀⠀⠀⢠⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⠀⠀⠀⠀⠀⠀⠀⢠⠀⠲⣄⡸⣿⣧⠀⠀⠀⠀⠙⢻⣿⣵⡙⣇⢿⣿⣦⣴⣷⣿⢟⣥⣶⣇⠀⠀⠀⠀⢈⡴⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n}⠀⠀⠀⠀⠀⠀⠀⠀⠀⡄⠻⣿⣦⣯⣇⣴⣴⣶⣾⣶⣿⣿⣿⡼⣿⣿⣿⣿⣿⣷⣿⣯⣯⣇⣤⣠⣤⣶⡼⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⠀⠀⠀⠀⠀⠀⠀⠘⠀⠈⠀⠹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⡡⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⡎⠀⠀⣀⠐⠀⠀⠀⠈⠀⠀⠀⠘⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀\n⣴⣶⣿⣿⠇⠀⠀⠀⠀⠀⠀⠀⠈⢳⣽⣿⣿⣿⣿⣿⣿⣿⣿⡿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣛⠛⠁⠀⠀⠀⠀⢀⠀⠀⠀⠀⠀⠘⢿\n⣿⣿⣿⠇⠠⣀⠀⣠⡾⠀⠀⠀⠀⠀⠙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠁⠀⠀⠀⠀⣦⡠⣀⠀⠀⠀⠀⠀⠀\n⣿⣿⣟⣴⣿⡟⣼⣿⡿⠀⠀⠀⠀⠀⠀⢀⠈⠙⠿⣿⣿⣿⣿⣿⣭⣟⣿⣿⣿⣿⣿⡿⠟⠉⣠⡀⠀⣴⣦⡀⠀⣿⣿⣮⣻⣿⣶⣶⣾⣿\n⣿⣿⣿⣿⣿⣿⣿⣛⣥⣴⣶⢏⣠⣴⣾⢇⣤⣾⣷⣦⠉⠛⢿⣿⣿⣿⣿⣿⣿⠟⠋⠀⠀⢠⣿⣿⣄⢸⣿⣷⣦⡘⣿⣿⣾⣿⣿⣿⣿⣿\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣿⣿⣿⣿⣿⠆⢀⠀⠈⠉⠉⠉⠁⠀⠀⠀⠀⠀⣼⣿⣿⣿⣧⣻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣅⢕⠔⢄⠀⠀⠀⠀⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢟⣿⣮⢷⣕⣄⠢⡀⡀⠀⠀⠀⠀⣸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿\n⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠿⠟⠁⢀⣿⣿⣿⣷⣿⣮⢳⣝⢦⡂⡐⠀⠀⠏⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿\n⣿⣿⣿⣿⣿⣿⣿⣿⠿⠟⢋⠉⠁⠀⠀⠀⢀⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣽⣷⣇⠀⢰⠀⠀⠋⠛⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿");
