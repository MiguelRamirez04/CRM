using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

namespace CRM.Config;

public static class ValidationConfiguration
{
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        // Agregar FluentValidation a ASP.NET Core
        services.AddFluentValidationAutoValidation(config =>
        {
            // Deshabilitar validación de DataAnnotations para evitar duplicados
            config.DisableDataAnnotationsValidation = false;
        });

        // Registrar todos los validadores automáticamente desde el assembly actual
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Configurar el comportamiento global de validación
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        return services;
    }
}
