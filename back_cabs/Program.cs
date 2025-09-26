using CRM.Config;
using Serilog;
using back_cabs.services;
using back_cabs.middleware;

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

// Registrar servicios personalizados
builder.Services.AddScoped<ServicioJwt>();

// Registrar servicios CRM de autenticación
builder.Services.AddScoped<back_cabs.CRM.services.Auth.UsuarioAuthService>();
builder.Services.AddScoped<back_cabs.CRM.validators.Auth.UsuarioRegistroValidator>();

// CORS con configuración de seguridad avanzada
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1");
        c.RoutePrefix = "swagger";
        c.EnableTryItOutByDefault();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
    });
}

// Middleware pipeline con seguridad mejorada
app.UseHttpsRedirection();

// Middleware de manejo de errores
app.UseMiddleware<MiddlewareManejoErrores>();

// Headers de seguridad
app.Use(async (context, next) =>
{
    // Prevenir clickjacking
    context.Response.Headers["X-Frame-Options"] = "DENY";
    
    // Prevenir MIME sniffing
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    
    // XSS Protection
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    
    // Content Security Policy
    context.Response.Headers["Content-Security-Policy"] = 
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
    
    // HSTS (solo en producción)
    if (app.Environment.IsProduction())
    {
        context.Response.Headers["Strict-Transport-Security"] = 
            "max-age=31536000; includeSubDomains";
    }
    
    // Referrer Policy
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    await next();
});

// CORS (usar política apropiada según el entorno)
var corsPolicy = app.Environment.IsProduction() ? "Production" : "SecureFrontend";
app.UseCors(corsPolicy);

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.UseHealthChecksConfiguration();

// Controladores
app.MapControllers();

// Logging de inicio
app.Logger.LogInformation("🚀 CRM API iniciada correctamente");

app.Run();
