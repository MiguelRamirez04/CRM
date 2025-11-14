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
using back_cabs.CRM.middleware;
using StackExchange.Redis;
using back_cabs.CRM.Interfaces;
using back_cabs.CRM.services.shared;
using back_cabs.CRM.Repositories;
using back_cabs.CRM.hubs;
using Microsoft.Data.SqlClient;

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

// ✅ Registrar IHttpContextAccessor para acceder al usuario autenticado en repositorios
builder.Services.AddHttpContextAccessor();

// Agregar configuraciones centralizadas
builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddValidationConfiguration();
builder.Services.AddMediatRConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// ✅ MEJORA 5: Configurar Anti-Forgery (CSRF Protection)
builder.Services.AddAntiforgery(options =>
{
    // Nombre del header donde el cliente enviará el token
    options.HeaderName = "X-XSRF-TOKEN";
    
    // Nombre de la cookie que almacenará el token
    options.Cookie.Name = "XSRF-TOKEN";
    
    // CRÍTICO: HttpOnly=false para que JavaScript pueda leer el token
    // Esto es seguro porque el token no es sensible por sí mismo
    options.Cookie.HttpOnly = false;
    
    // Secure=None para desarrollo (permitir HTTP), cambiar a Always en producción
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    
    // SameSite=Lax para desarrollo, cambiar a Strict en producción
    options.Cookie.SameSite = SameSiteMode.Lax;
    
    // Path raíz para disponibilidad en toda la app
    options.Cookie.Path = "/";
});

// Servicios básicos de ASP.NET Core
builder.Services.AddControllers();

// Configurar SignalR para notificaciones en tiempo real con autenticación
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Para desarrollo
    options.MaximumReceiveMessageSize = 102400; // 100KB
});

// Inyección de contextos de base de datos
builder.Services.AddDbContext<ReadOnlyContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<WriteContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ═══════════════════════════════════════════════════════════════
// CONTEXTO LEGACY COMPAC - Base de datos adCABS2016
// ═══════════════════════════════════════════════════════════════
builder.Services.AddDbContext<LegacyCompacReadOnlyContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CompacConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30); // 30 segundos para consultas complejas
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        }
    )
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)); // Solo lectura

// Contexto de escritura (usar con precaución - datos legacy)
builder.Services.AddDbContext<LegacyCompacWriteContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CompacConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(30); // 30 segundos para operaciones de escritura
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        }
    )); // Change Tracking habilitado por defecto

// ═══════════════════════════════════════════════════════════════
// UNIT OF WORK PATTERN
// ═══════════════════════════════════════════════════════════════
// Coordina transacciones entre múltiples repositorios
// Garantiza atomicidad (todo o nada) en operaciones complejas
builder.Services.AddScoped<back_cabs.CRM.Core.UnitOfWork.IUnitOfWork, back_cabs.CRM.Core.UnitOfWork.UnitOfWork>();

// ═══════════════════════════════════════════════════════════════
// REPOSITORIOS (REPOSITORY PATTERN)
// ═══════════════════════════════════════════════════════════════
// Nota: Los repositorios se mantienen para uso directo cuando no se requiere transacción
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Shared.IVehiculoRepository, back_cabs.CRM.repositories.Shared.VehiculoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Recepcion.IEjecucionOrdenRepository, back_cabs.CRM.repositories.Recepcion.EjecucionOrdenRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Auth.IUsuarioAuthRepository, back_cabs.CRM.repositories.Auth.UsuarioAuthRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Recepcion.ICotizacionRepository, back_cabs.CRM.repositories.Recepcion.CotizacionRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Recepcion.IOrdenTrabajoRepository, back_cabs.CRM.repositories.Recepcion.OrdenTrabajoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.IClientesLegacyValidationRepository, back_cabs.CRM.repositories.ClientesLegacyValidationRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Soporte.IReparacionRepository, back_cabs.CRM.repositories.Soporte.ReparacionRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAgenteRepository, back_cabs.CRM.repositories.Legacy.AdmAgenteRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMonedaRepository, back_cabs.CRM.repositories.Legacy.AdmMonedaRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAlmacenRepository, back_cabs.CRM.repositories.Legacy.AdmAlmacenRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmProductoRepository, back_cabs.CRM.repositories.Legacy.AdmProductoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoModeloRepository, back_cabs.CRM.repositories.Legacy.AdmDocumentoModeloRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmConceptoRepository, back_cabs.CRM.repositories.Legacy.AdmConceptoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmNumeroSerieRepository, back_cabs.CRM.repositories.Legacy.AdmNumeroSerieRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoRepository, back_cabs.CRM.repositories.Legacy.AdmDocumentoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmClienteRepository, back_cabs.CRM.repositories.Legacy.AdmClienteRepository>();

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

// Registro de servicios Legacy (solo conexión directa a BD legacy adCABS2016)
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAgenteService, back_cabs.CRM.services.Legacy.AdmAgenteService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMonedaService, back_cabs.CRM.services.Legacy.AdmMonedaService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmAlmacenService, back_cabs.CRM.services.Legacy.AdmAlmacenService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmProductoService, back_cabs.CRM.services.Legacy.AdmProductoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoModeloService, back_cabs.CRM.services.Legacy.AdmDocumentoModeloService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmConceptoService, back_cabs.CRM.services.Legacy.AdmConceptoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmNumeroSerieService, back_cabs.CRM.services.Legacy.AdmNumeroSerieService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmDocumentoService, back_cabs.CRM.services.Legacy.AdmDocumentoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmClienteService, back_cabs.CRM.services.Legacy.AdmClienteService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmUnidadMedidaPesoRepository, back_cabs.CRM.repositories.Legacy.AdmUnidadMedidaPesoRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmUnidadMedidaPesoService, back_cabs.CRM.services.Legacy.AdmUnidadMedidaPesoService>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMovimientoSerieRepository, back_cabs.CRM.repositories.Legacy.AdmMovimientoSerieRepository>();
builder.Services.AddScoped<back_cabs.CRM.Interfaces.Legacy.IAdmMovimientoSerieService, back_cabs.CRM.services.Legacy.AdmMovimientoSerieService>();

//Interfaces del los servicios que acabamos de realizar
// Registra el repositorio para que el servicio pueda usarlo
builder.Services.AddScoped<back_cabs.CRM.Interfaces.IDetalleEvaluacionRepository, back_cabs.CRM.Repositories.DetalleEvaluacionRepository>();
builder.Services.AddScoped<IGastoViaticoRepository, GastoViaticoRepository>();
builder.Services.AddScoped<IGastoViaticoService, back_cabs.CRM.Services.Shared.GastoViaticoService>();
// Servicio de depuración para problemas de clientes legacy
builder.Services.AddScoped<back_cabs.CRM.services.ClientesLegacyValidationService>();
builder.Services.AddScoped<back_cabs.CRM.services.shared.EvaluacionService>();
builder.Services.AddScoped<back_cabs.CRM.services.shared.FotosEvaluacionService>();

// Registrar servicio en segundo plano para expiración de cotizaciones
// builder.Services.AddHostedService<back_cabs.CRM.services.Background.CotizacionExpirationService>();

// Servicios de procesamiento de imágenes y gestión de archivos
builder.Services.AddScoped<back_cabs.CRM.services.shared.ImageProcessingService>();
builder.Services.AddScoped<back_cabs.CRM.services.Soporte.ReparacionFotoService>();

// Servicio genérico de almacenamiento de archivos
builder.Services.AddScoped<back_cabs.CRM.services.Files.IFileStorageService, back_cabs.CRM.services.Files.FileStorageService>();

// Servicio de notificaciones con SignalR
builder.Services.AddScoped<back_cabs.CRM.services.INotificacionService, back_cabs.CRM.services.NotificacionService>();

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
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:4201", "https://localhost:4201", "http://localhost:5176", "https://localhost:5176") // Angular
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

// Servir archivos estáticos (para Swagger UI custom scripts)
app.UseStaticFiles();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// ✅ MEJORA 5: Activar validación CSRF (debe ir DESPUÉS de auth)
app.UseCsrfValidation();

// Swagger UI (solo en desarrollo y staging)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API v1");
        c.RoutePrefix = "swagger";
        // Inyectar script para manejar CSRF token automáticamente
        c.InjectJavascript("/swagger-ui/csrf-interceptor.js");
    });
}

// Health checks
app.UseHealthChecksConfiguration();

// Controladores
app.MapControllers();

// Configurar SignalR Hub para notificaciones
app.MapHub<NotificacionesHub>("/hubs/notificaciones");

// Logging de inicio
app.Logger.LogInformation("🚀 CRM API iniciada correctamente");

try
{
    app.Run();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Error fatal en la aplicación");
    throw;
}


