# Routes - Configuración de Rutas

Configuración avanzada de rutas cuando se necesita más control que los atributos de controladores.

## 📋 Responsabilidades

- Configurar rutas personalizadas
- Agrupar endpoints relacionados
- Aplicar middleware específico a rutas
- Configurar versionado de API

## 🔗 Conexiones

- **Se configura en**: Program.cs
- **Mapea a**: Controllers y Actions
- **Puede usar**: Middleware específico
- **Permite**: Versionado y agrupación

## 🛠️ Casos de Uso

### Agrupación de Rutas
- Rutas de administración: `/admin/*`
- Rutas de API pública: `/api/v1/*`
- Rutas de soporte: `/support/*`

### Middleware Específico
- Autenticación solo en ciertas rutas
- Rate limiting por grupo de endpoints
- Logging específico por módulo

## 💡 Ejemplo de Configuración

```csharp
// En Program.cs
app.MapGroup("/api/v1/admin")
   .RequireAuthorization("AdminPolicy")
   .WithTags("Administración");

app.MapGroup("/api/v1/public")
   .AllowAnonymous()
   .WithTags("Público");
```

## 📁 Organización

- `AdminRoutes.cs` - Rutas de administración
- `PublicRoutes.cs` - Rutas públicas
- `SupportRoutes.cs` - Rutas de soporte
- `ApiVersioning.cs` - Configuración de versionado

## ⚙️ Características

- Soporte para versionado
- Agrupación lógica
- Middleware condicional
- Documentación automática en Swagger

**Nota**: En muchos casos, los atributos en controladores son suficientes. Esta carpeta es para casos avanzados.