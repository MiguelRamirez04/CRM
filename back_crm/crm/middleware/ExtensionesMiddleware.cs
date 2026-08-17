// =================================================================================================
// EXTENSIONES DE MIDDLEWARE - ExtensionesMiddleware.cs
// =================================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Este archivo proporciona métodos de extensión para IApplicationBuilder que facilitan
// la configuración y registro de middlewares personalizados en el pipeline de ASP.NET Core.
// Actúa como un punto centralizado para la configuración de funcionalidades transversales
// como manejo de errores, seguridad, logging y otras capacidades del middleware.
//
// FUNCIONALIDADES PRINCIPALES:
// - Registro simplificado del middleware de manejo de errores global
// - Configuración automática de headers de seguridad HTTP
// - Middleware de logging de requests y responses para debugging
// - Patrón de fluent interface para configuración de pipeline
//
// ¿CÓMO SE USA?
// 1. EN Program.cs o Startup.cs para configurar el pipeline HTTP:
//    app.UseGlobalErrorHandling()
//       .UseSecurityHeaders()
//       .UseRequestResponseLogging();
//
// 2. CONFIGURACIÓN SELECTIVA:
//    // Solo manejo de errores
//    app.UseGlobalErrorHandling();
//    // Agregar seguridad después
//    app.UseSecurityHeaders();
//
// ¿EN QUÉ CASOS SE USA?
// - CONFIGURACIÓN DE PIPELINE: En Program.cs para establecer middlewares personalizados
// - SEGURIDAD WEB: Para agregar headers de seguridad automáticamente a todas las responses
// - DEBUGGING EN DESARROLLO: Para loggear requests/responses durante desarrollo
// - MANEJO DE ERRORES: Para interceptar y procesar excepciones no manejadas
// - AUDITORÍA: Para registrar actividad de requests en logs
//
// EJEMPLOS PRÁCTICOS:
//
// 1. CONFIGURACIÓN COMPLETA EN Program.cs:
//    var app = builder.Build();
//    app.UseGlobalErrorHandling();      // Primero: capturar todos los errores
//    app.UseSecurityHeaders();          // Segundo: agregar headers de seguridad
//    app.UseRequestResponseLogging();   // Tercero: logging de requests (desarrollo)
//    app.UseRouting();
//    app.UseAuthorization();
//    app.MapControllers();
//
// 2. CONFIGURACIÓN MÍNIMA PARA PRODUCCIÓN:
//    app.UseGlobalErrorHandling();
//    app.UseSecurityHeaders();
//    // Sin logging de requests en producción por performance
//
// HEADERS DE SEGURIDAD CONFIGURADOS:
// - X-Frame-Options: DENY (previene clickjacking)
// - X-Content-Type-Options: nosniff (previene MIME sniffing)
// - X-XSS-Protection: 1; mode=block (protección XSS básica)
// - Content-Security-Policy: default-src 'self' (CSP básica)
// - Referrer-Policy: strict-origin-when-cross-origin (control de referrer)
//
// INTEGRACIÓN CON OTROS COMPONENTES:
// - Funciona con MiddlewareManejoErrores para respuestas de error consistentes
// - Compatible con sistemas de logging como Serilog
// - Se integra con el pipeline estándar de ASP.NET Core
// - Compatible con middleware de autenticación y autorización
//
// VENTAJAS DE USAR ESTE SISTEMA:
// - CENTRALIZACIÓN: Toda configuración de middleware en un solo lugar
// - CONSISTENCIA: Configuración uniforme en diferentes entornos
// - MANTENIBILIDAD: Cambios de configuración sin modificar Startup/Program
// - REUTILIZACIÓN: Extensiones reutilizables en múltiples proyectos
// - TESTABILIDAD: Configuración de middleware testeable
//
// DEPENDENCIAS EXTERNAS:
// - Microsoft.AspNetCore.Builder (para IApplicationBuilder)
// - Microsoft.AspNetCore.Http (para HttpContext)
// - Microsoft.Extensions.Logging (para logging)
//
// NOTAS DE IMPLEMENTACIÓN:
// - Métodos de extensión thread-safe para uso concurrente
// - Configuración idempotente (puede llamarse múltiples veces sin efectos secundarios)
// - Compatible con el patrón de middleware de ASP.NET Core
// - Headers de seguridad siguen mejores prácticas OWASP

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace back_cabs.CRM.Middleware;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Agrega el middleware de manejo de errores global al pipeline
    /// </summary>
    /// <param name="app">El constructor de aplicación ASP.NET Core</param>
    /// <returns>El constructor de aplicación con el middleware agregado</returns>
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
    {
        // REGISTRO DEL MIDDLEWARE EN EL PIPELINE HTTP:
        // Utiliza el método UseMiddleware<T> de ASP.NET Core para registrar
        // la clase MiddlewareManejoErrores en el pipeline de procesamiento de requests
        //
        // POSICIONAMIENTO EN EL PIPELINE:
        // Este middleware debe registrarse TEMPRANO en el pipeline (cerca del inicio)
        // para capturar excepciones de todos los middlewares posteriores
        //
        // FUNCIONAMIENTO:
        // - Intercepta cualquier excepción no manejada que ocurra downstream
        // - Convierte excepciones en respuestas HTTP JSON estructuradas
        // - Registra errores en el sistema de logging configurado
        // - Mantiene consistencia en el formato de respuestas de error
        return app.UseMiddleware<MiddlewareManejoErrores>();
    }

    /// <summary>
    /// Agrega headers de seguridad básicos a todas las respuestas HTTP
    /// siguiendo las mejores prácticas de OWASP y seguridad web moderna
    /// </summary>
    /// <param name="app">El constructor de aplicación ASP.NET Core</param>
    /// <returns>El constructor de aplicación con headers de seguridad configurados</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (!context.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            }

            await next();
        });
    }

    /// <summary>
    /// Agrega middleware para logging detallado de requests y responses HTTP
    /// Útil para debugging, auditoría y monitoreo de la aplicación en desarrollo
    /// </summary>
    /// <param name="app">El constructor de aplicación ASP.NET Core</param>
    /// <returns>El constructor de aplicación con logging de requests configurado</returns>
    /// <remarks>
    /// NOTA: Este middleware genera logs detallados que pueden afectar el rendimiento
    /// en producción. Se recomienda usarlo solo en entornos de desarrollo o con
    /// configuración condicional basada en el environment.
    /// </remarks>
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        // CONFIGURACIÓN DE MIDDLEWARE DE LOGGING:
        // Middleware personalizado que registra información detallada de requests y responses
        // Útil para debugging, auditoría y monitoreo durante desarrollo
        // ADVERTENCIA: Este middleware puede impactar el rendimiento en producción
        return app.Use(async (context, next) =>
        {
            // OBTENCIÓN DEL LOGGER DESDE EL CONTENEDOR DE DEPENDENCIAS:
            // Utiliza GetRequiredService para obtener ILogger inyectado por ASP.NET Core
            // Usa ILogger<MiddlewareManejoErrores> para consistencia con otros componentes
            // Si el servicio no está registrado, lanza excepción (comportamiento esperado)
            var logger = context.RequestServices.GetRequiredService<ILogger<MiddlewareManejoErrores>>();

            // LOGGING DEL REQUEST ENTRANTE:
            // Registra información básica del request ANTES de procesarlo
            // Incluye método HTTP (GET, POST, etc.), ruta solicitada y IP del cliente
            // Nivel Information: Adecuado para seguimiento normal de requests
            // Formato estructurado facilita consultas y filtros en sistemas de logging
            logger.LogInformation("Request: {Method} {Path} from {RemoteIp}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            // EJECUCIÓN DEL PIPELINE DE MIDDLEWARE:
            // Llama a await next() para ejecutar todos los middlewares posteriores
            // Esto incluye controladores, autorización, autenticación, etc.
            // El request se procesa completamente antes de continuar con el logging
            await next();

            // LOGGING DE LA RESPONSE:
            // Después de procesar el request, registra el código de estado HTTP
            // Permite identificar requests exitosos (2xx), redirecciones (3xx), errores cliente (4xx), errores servidor (5xx)
            // Información correlacionada con el request original para tracing completo
            logger.LogInformation("Response: {StatusCode} for {Method} {Path}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path);
        });
    }
}