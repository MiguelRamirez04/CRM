using FluentValidation;
using System.Reflection;

namespace CRM.Config;

public static class ValidationConfiguration
{
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        // Registrar todos los validadores automáticamente
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Configurar el comportamiento global de validación
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        return services;
    }
}