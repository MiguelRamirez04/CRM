using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

namespace CRM.Config;

public static class ValidationConfiguration
{
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
<<<<<<< .merge_file_5N6UR4
        // ⚠️ NOTA: AddFluentValidationAutoValidation NO se usa porque:
        // 1. El pipeline automático de ASP.NET Core es SÍNCRONO
        // 2. Nuestros validadores contienen reglas ASÍNCRONAS (MustAsync) para validar contra BD
        // 3. Solución: Validación MANUAL en servicios (ej: UsuarioAuthService.RegistrarUsuarioAsync)
        
=======

>>>>>>> .merge_file_d4svwn
        // Registrar todos los validadores automáticamente desde el assembly actual
        // Estos se inyectan manualmente en servicios para validación asíncrona
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Configurar el comportamiento global de validación
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        return services;
    }
}

