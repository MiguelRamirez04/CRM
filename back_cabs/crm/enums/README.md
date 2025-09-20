# Enums - Enumeraciones

Enumeraciones que definen valores constantes usados en todo el sistema. Similar a enums en TypeScript.

## 📋 Responsabilidades

- Definir conjuntos de valores constantes
- Proporcionar tipo seguro para opciones
- Evitar "magic strings" en el código
- Facilitar mantenimiento y refactoring

## 🔗 Conexiones

- **Usado por**: Models, DTOs, Services
- **Puede tener**: Extensiones en Utils
- **Se mapea a**: Base de datos como strings o ints
- **Documentado en**: Swagger automáticamente

## 🛠️ Tipos de Enums

### Estados del Sistema
- `EstadoTicket.cs` - Estados de tickets (Abierto, EnProceso, Resuelto, Cerrado)
- `EstadoPedido.cs` - Estados de pedidos (Pendiente, Procesando, Completado, Cancelado)
- `EstadoEmpleado.cs` - Estados de empleados (Activo, Inactivo, Suspendido)

### Prioridades y Niveles
- `PrioridadTicket.cs` - Prioridades (Baja, Media, Alta, Crítica)
- `NivelAcceso.cs` - Niveles de acceso al sistema

### Categorías
- `CategoriaTicket.cs` - Categorías de tickets
- `TipoPedido.cs` - Tipos de pedidos

## 💡 Ejemplo de Estructura

```csharp
public enum EstadoTicket
{
    Abierto = 1,
    EnProceso = 2,
    Resuelto = 3,
    Cerrado = 4
}

public enum PrioridadTicket
{
    Baja = 1,
    Media = 2,
    Alta = 3,
    Critica = 4
}
```

## 🎨 Con Atributos Descriptivos

```csharp
public enum EstadoTicket
{
    [Description("Ticket abierto y sin asignar")]
    Abierto = 1,
    
    [Description("Ticket en proceso de resolución")]
    EnProceso = 2,
    
    [Description("Ticket resuelto, pendiente de cerrar")]
    Resuelto = 3,
    
    [Description("Ticket cerrado definitivamente")]
    Cerrado = 4
}
```

## ⚙️ Buenas Prácticas

- Usar valores explícitos (1, 2, 3...)
- Nombres en PascalCase
- Incluir atributos Description para UI
- Agrupar enums relacionados
- Considerar compatibilidad con base de datos