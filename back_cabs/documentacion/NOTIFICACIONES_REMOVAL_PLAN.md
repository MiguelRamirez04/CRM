# Eliminar Sistema de Notificaciones

Eliminación completa del sistema de notificaciones SignalR para resolver problemas de despliegue en Windows Server.

## User Review Required

> [!WARNING]
> **Eliminación Permanente**: Esta operación eliminará completamente el sistema de notificaciones en tiempo real. Los archivos y funcionalidades serán removidos permanentemente.

> [!IMPORTANT]
> **Servicio de Notificaciones UI**: El servicio `NotificationService` en el frontend (usado para snackbars/toasts) **NO** será eliminado ya que es utilizado por múltiples componentes para mostrar mensajes de éxito/error. Solo se eliminará la interfaz `notificacion.interface.ts` relacionada con SignalR.

## Proposed Changes

### Backend - Archivos a Eliminar

#### [DELETE] [Notificacion.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/models/Notificacion.cs)
Modelo de entidad para notificaciones en base de datos.

#### [DELETE] [NotificacionesHub.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/hubs/NotificacionesHub.cs)
Hub de SignalR para comunicación en tiempo real.

#### [DELETE] [NotificacionService.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/services/NotificacionService.cs)
Servicio de lógica de negocio para notificaciones.

#### [DELETE] [INotificacionService.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/services/NotificacionService/INotificacionService.cs)
Interfaz del servicio de notificaciones.

#### [DELETE] [NotificacionDto.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/DTOs/Response/NotificacionDto.cs)
DTO para transferencia de datos de notificaciones.

#### [DELETE] [NotificacionesController.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/controllers/NotificacionesController.cs)
Controlador API para endpoints de notificaciones.

---

### Backend - Archivos a Modificar

#### [MODIFY] [Program.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/Program.cs)
- **Línea 22**: Remover `using back_cabs.CRM.hubs;`
- **Líneas 97-102**: Remover configuración de SignalR
- **Línea 213**: Remover registro del servicio `INotificacionService`
- **Línea 337**: Remover endpoint del hub `/hubs/notificaciones`

#### [MODIFY] [WriteContext.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/contexts/WriteContext.cs)
- **Línea 116**: Remover `DbSet<Notificacion> Notificaciones`

#### [MODIFY] [ReadOnlyContext.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/contexts/ReadOnlyContext.cs)
- **Línea 119**: Remover `DbSet<Notificacion> Notificaciones`

#### [MODIFY] [EjecucionOrdenService.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/crm/services/shared/EjecucionOrdenService.cs)
- **Línea 29**: Remover campo `_notificacionService`
- **Línea 36**: Remover parámetro del constructor `INotificacionService`
- Remover todas las llamadas a `_notificacionService` (delegación, finalización, etc.)

---

### Frontend - Archivos a Eliminar

#### [DELETE] [notificacion.interface.ts](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/front_cabs/src/app/core/models/notificacion.interface.ts)
Interfaces TypeScript para notificaciones SignalR.

---

### Frontend - Archivos a Mantener

> [!NOTE]
> **NotificationService NO se eliminará**: El archivo `notification.service.ts` se mantiene ya que es usado por componentes como `vehiculos-dialog.component.ts` para mostrar mensajes toast/snackbar (success, error, warning, info) usando Angular Material.

## Verification Plan

### Automated Tests

No existen tests automatizados específicos para el sistema de notificaciones que necesiten ser actualizados.

### Manual Verification

1. **Compilar Backend**
   ```powershell
   cd c:\Users\ANA\Documents\dev\FullStack_CABS\back_cabs
   dotnet build
   ```
   ✅ Debe compilar sin errores relacionados con notificaciones

2. **Compilar Frontend**
   ```powershell
   cd c:\Users\ANA\Documents\dev\FullStack_CABS\front_cabs
   npm run build
   ```
   ✅ Debe compilar sin errores de imports faltantes

3. **Ejecutar Backend**
   ```powershell
   cd c:\Users\ANA\Documents\dev\FullStack_CABS\back_cabs
   dotnet run
   ```
   ✅ No debe intentar mapear el hub de notificaciones
   ✅ No debe haber errores de SignalR en los logs

4. **Verificar Funcionalidad Existente**
   - Probar registro de usuarios
   - Probar creación de vehículos
   - Verificar que los mensajes toast/snackbar siguen funcionando (usan `NotificationService` que se mantiene)

5. **Verificar Logs**
   - No deben aparecer referencias a `NotificacionesHub`
   - No deben aparecer errores de conexión SignalR
