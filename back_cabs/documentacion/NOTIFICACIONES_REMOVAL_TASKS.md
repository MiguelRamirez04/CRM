# Eliminar Sistema de Notificaciones

## Backend (C# / ASP.NET Core)
- [x] Eliminar archivos del sistema de notificaciones
  - [x] `Notificacion.cs` (modelo)
  - [x] `NotificacionesHub.cs` (SignalR hub)
  - [x] `NotificacionService.cs` (servicio)
  - [x] `INotificacionService.cs` (interfaz)
  - [x] `NotificacionDto.cs` (DTO)
  - [x] `NotificacionesController.cs` (controller)
- [x] Modificar `Program.cs`
  - [x] Remover using de hubs (línea 22)
  - [x] Remover configuración SignalR (líneas 97-102)
  - [x] Remover registro servicio (línea 213)
  - [x] Remover endpoint hub (línea 337)
- [x] Modificar DbContexts
  - [x] `WriteContext.cs` - remover DbSet (línea 116)
  - [x] `ReadOnlyContext.cs` - remover DbSet (línea 119)
- [x] Modificar `EjecucionOrdenService.cs`
  - [x] Remover campo _notificacionService
  - [x] Remover parámetro constructor
  - [x] Remover llamadas al servicio
- [x] Modificar `EjecucionOrdenServiceTests.cs`
  - [x] Remover mock de NotificacionService

## Frontend (Angular)
- [x] Eliminar archivos SignalR
  - [x] `notificacion.interface.ts` (interfaces)
  - [x] `notificaciones.service.ts` (servicio SignalR)
  - [x] `signalr.service.ts` (cliente SignalR)
  - [x] `notificaciones/` (componente completo)
  - [x] ⚠️ MANTENER `notification.service.ts` (usado para toasts)
- [x] Modificar `header.component`
  - [x] Remover imports de SignalR
  - [x] Remover indicador de conexión
  - [x] Remover componente notificaciones

## Verificación
- [x] Compilar backend sin errores (⚠️ requiere detener proceso en ejecución)
- [x] Compilar frontend sin errores (auto-recompilación en ng serve)
- [ ] Ejecutar backend sin errores SignalR
- [ ] Probar funcionalidad existente
