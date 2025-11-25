# Uploads - Archivos Subidos

Almacenamiento y gestión de archivos subidos por usuarios. Similar a carpetas public/uploads en Node.js.

## 📋 Responsabilidades

- Almacenar archivos subidos temporalmente
- Organizar archivos por tipo y módulo
- Proporcionar acceso controlado a archivos
- Gestionar límites de tamaño y tipos

## 🔗 Conexiones

- **Recibe archivos de**: Controllers (multipart/form-data)
- **Puede procesar con**: Services especializados
- **Se configura en**: Program.cs (static files)
- **Accesible via**: URLs públicas o controladas

## 📁 Organización por Módulos

### Administracion/
- `empleados/` - Fotos de empleados
- `documentos/` - Documentos administrativos

### Recepcion/
- `clientes/` - Documentos de clientes
- `pedidos/` - Archivos adjuntos a pedidos

### Soporte/
- `tickets/` - Archivos adjuntos de tickets
- `capturas/` - Capturas de pantalla

### Temporal/
- `temp/` - Archivos temporales para procesamiento

## 🛡️ Seguridad

### Validaciones
- Tipos de archivo permitidos
- Tamaño máximo por archivo
- Escaneo de virus (opcional)
- Validación de contenido

### Acceso Controlado
```csharp
[HttpGet("download/{ticketId}/{fileName}")]
[Authorize]
public async Task<IActionResult> DownloadFile(int ticketId, string fileName)
{
    // Validar permisos del usuario
    // Devolver archivo si tiene acceso
}
```

## 💡 Ejemplo de Configuración

```csharp
// En Program.cs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "CRM/uploads")),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx => {
        // Configurar headers de seguridad
    }
});
```

## ⚙️ Buenas Prácticas

- Nunca ejecutar archivos subidos
- Validar tipos MIME reales
- Generar nombres únicos para archivos
- Límites de cuota por usuario/módulo
- Backup regular de archivos importantes
- Limpieza automática de archivos temporales

## 🔧 Procesamiento

- Redimensionamiento de imágenes
- Conversión de documentos
- Extracción de metadatos
- Compresión automática