# Scripts - Scripts de Utilidad

Scripts para automatizar tareas de desarrollo y mantenimiento. Similar a scripts en package.json de Node.js.

## 📋 Responsabilidades

- Automatizar tareas repetitivas
- Scripts de inicialización de datos
- Herramientas de desarrollo
- Utilerías de base de datos

## 🔗 Conexiones

- **Ejecutados por**: Desarrolladores, CI/CD
- **Pueden usar**: Entity Framework, Services
- **Acceden a**: Base de datos, archivos del sistema
- **Se ejecutan**: Via dotnet run o PowerShell

## 🛠️ Tipos de Scripts

### Inicialización de Datos
- `SeedData.cs` - Datos iniciales para desarrollo
- `CreateAdminUser.cs` - Crear usuario administrador
- `SampleData.cs` - Datos de ejemplo para pruebas

### Base de Datos
- `DatabaseMigration.cs` - Ejecutar migraciones
- `DatabaseBackup.cs` - Respaldo de base de datos
- `CleanupDatabase.cs` - Limpiar datos obsoletos

### Desarrollo
- `GenerateTestData.cs` - Generar datos de prueba
- `ExportSchema.cs` - Exportar esquema de BD
- `ValidateConfiguration.cs` - Validar configuraciones

## 💡 Ejemplo de Estructura

```csharp
public class SeedDataScript
{
    private readonly WriteContext _context;
    
    public async Task ExecuteAsync()
    {
        // Crear roles por defecto
        await CreateDefaultRoles();
        
        // Crear usuario administrador
        await CreateAdminUser();
        
        // Crear categorías de tickets
        await CreateTicketCategories();
    }
}
```

## 🔧 Ejecución

### Desde Código
```csharp
// En Program.cs para desarrollo
if (app.Environment.IsDevelopment())
{
    await SeedDataScript.ExecuteAsync();
}
```

### Como Herramientas
- `dotnet run --script SeedData`
- Scripts de PowerShell para automatización
- Integración con CI/CD pipelines

## 📁 Organización

- `Development/` - Scripts solo para desarrollo
- `Production/` - Scripts seguros para producción
- `Database/` - Scripts específicos de BD
- `Utilities/` - Herramientas generales

## ⚙️ Consideraciones

- Documentar qué hace cada script
- Validar ambiente antes de ejecutar
- Logs detallados de ejecución
- Rollback cuando sea posible