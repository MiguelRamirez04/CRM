# CRM - Estructura Simplificada

Este monorepo contiene un CRM con arquitectura hexagonal simplificada.

## Estructura Principal

```
crm/
├── src/
│   ├── Api/                     # API Web y SignalR 
│   ├── Core/                    # Domain + Application (fusionados)
│   └── Infrastructure/          # Persistencia y adaptadores
├── tests/                       # Todas las pruebas
└── docs/                        # Documentación
```

## Principios de Arquitectura

1. **Core (Domain + Application):** Lógica de negocio y casos de uso por módulo
2. **Infrastructure:** Implementación de persistencia, notificaciones, etc.
3. **Api:** Controllers, SignalR Hubs y configuración mínima
4. **Tests:** Pruebas organizadas por módulo y tipo

## Módulos CRM

- **Administracion:** Gestión de empleados y configuración
- **Recepcion:** Creación y gestión inicial de pedidos  
- **Soporte:** Cola de soporte y tiempo real (SignalR)

## Para Empezar a Codificar

1. Definir entidades en `Core/{Modulo}/Domain/`
2. Crear comandos/queries en `Core/{Modulo}/Application/`
3. Implementar repositorios en `Infrastructure/{Modulo}/`
4. Crear controllers en `Api/Controllers/`
5. Configurar hubs en `Api/Hubs/` (principalmente Soporte)

## Próximos Pasos

- [ ] Definir entidades de dominio con propiedades
- [ ] Implementar handlers con lógica real
- [ ] Configurar Entity Framework y migraciones
- [ ] Implementar SignalR para tiempo real
- [ ] Agregar validaciones y DTOs