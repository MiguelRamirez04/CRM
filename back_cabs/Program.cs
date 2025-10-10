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

var builder = WebApplication.CreateBuilder(args);

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

// Inyección de servicios de la aplicación
builder.Services.AddScoped<ServicioJwt>();
builder.Services.AddScoped<UsuarioAuthService>();
builder.Services.AddScoped<VehiculosService>();
builder.Services.AddScoped<ClientesCompletosService>();
builder.Services.AddScoped<back_cabs.CRM.services.Recepcion.OrdenTrabajoService>();
// Servicio de depuración para problemas de clientes legacy
builder.Services.AddScoped<back_cabs.CRM.services.ClientesLegacyValidationService>();

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


Console.WriteLine("Hello, World!");