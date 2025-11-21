using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

namespace CRM.Config;

public static class ValidationConfiguration
{
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
<<<<<<< HEAD

=======
        // ⚠️ NOTA: AddFluentValidationAutoValidation NO se usa porque:
        // 1. El pipeline automático de ASP.NET Core es SÍNCRONO
        // 2. Nuestros validadores contienen reglas ASÍNCRONAS (MustAsync) para validar contra BD
        // 3. Solución: Validación MANUAL en servicios (ej: UsuarioAuthService.RegistrarUsuarioAsync)
        
>>>>>>> 29afbe45571ab99f1c722a38a504c27ea9e3be5c
        // Registrar todos los validadores automáticamente desde el assembly actual
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Configurar el comportamiento global de validación
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        return services;
    }
}
