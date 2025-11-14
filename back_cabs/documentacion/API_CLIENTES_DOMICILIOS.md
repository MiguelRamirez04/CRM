# API de Clientes con Domicilios - Legacy Adminpaq

## 📋 Descripción

API RESTful para consultar clientes del sistema legacy Adminpaq (adCABS2016) con información completa de sus domicilios. Unifica datos de las tablas `admClientes` y `admDomicilios` en una sola respuesta.

## 🔗 Endpoints

### 1. Búsqueda Paginada de Clientes

**GET** `/api/AdmClientes/search`

Busca clientes con filtros múltiples y paginación.

#### Parámetros Query (todos opcionales)

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `codigoCliente` | string | null | Código del cliente (búsqueda parcial, case-insensitive) |
| `razonSocial` | string | null | Razón social/nombre (búsqueda parcial, case-insensitive) |
| `rfc` | string | null | RFC del cliente (búsqueda parcial, case-insensitive) |
| `email` | string | null | Email del cliente (búsqueda en email1, email2, email3) |
| `telefono` | string | null | Teléfono (búsqueda en todos los teléfonos registrados) |
| `estado` | string | null | Estado del domicilio (búsqueda parcial) |
| `ciudad` | string | null | Ciudad del domicilio (búsqueda parcial) |
| `estatus` | int | 1 | Estatus del cliente: `1` = Activo, `0` = Inactivo, `null` = Todos |
| `tipoDireccion` | int | 1 | Tipo de dirección: `1` = Fiscal, `2` = Envío, `3` = Otros, `null` = Todas |
| `incluirDetalleUbicacion` | bool | true | Incluir objeto detallado de ubicación |
| `numeroPagina` | int | 1 | Número de página (inicia en 1) |
| `tamanoPagina` | int | 50 | Registros por página (máximo 100) |

#### Ejemplo de Petición

```http
GET /api/AdmClientes/search?razonSocial=COMPUTACION&estado=Durango&estatus=1&incluirDetalleUbicacion=true
Authorization: Bearer {token}
```

#### Respuesta Exitosa (200 OK)

```json
{
  "success": true,
  "data": [
    {
      "id": 2017,
      "codigoCliente": "20 DEL VALLE",
      "nombre": "COMPUTACION EN ACCION, SA DE CV",
      "rfc": "VVA160219TB1",
      "telefono": "8250300",
      "email": "contacto@compuaccion.com",
      "ubicacion": "LAUREANO RONCAL SUR 611\nZONA CENTRO\nVictoria de Durango, Durango",
      "estado": "Durango",
      "ubicacionDetalle": {
        "calle": "LAUREANO RONCAL SUR",
        "numeroExterior": "611",
        "numeroInterior": "",
        "colonia": "ZONA CENTRO",
        "codigoPostal": "34000",
        "ciudad": "Victoria de Durango",
        "municipio": "Durango",
        "estado": "Durango",
        "pais": "MEXICO",
        "telefono1": "8250300",
        "telefono2": "",
        "telefonoCompleto": "8250300"
      }
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 50,
    "totalPages": 1,
    "totalRecords": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "filters": {
    "codigoCliente": null,
    "razonSocial": "COMPUTACION",
    "rfc": null,
    "email": null,
    "telefono": null,
    "estado": "Durango",
    "ciudad": null,
    "estatus": 1,
    "tipoDireccion": 1,
    "incluirDetalleUbicacion": true
  },
  "message": "Se encontraron 1 cliente(s)"
}
```

### 2. Obtener Cliente por ID

**GET** `/api/AdmClientes/{id}`

Obtiene un cliente específico por su ID con información completa de domicilio.

#### Parámetros

| Parámetro | Ubicación | Tipo | Descripción |
|-----------|-----------|------|-------------|
| `id` | Path | int | ID del cliente (CIDCLIENTEPROVEEDOR) |
| `incluirDetalleUbicacion` | Query | bool | Incluir detalle de ubicación (default: true) |

#### Ejemplo de Petición

```http
GET /api/AdmClientes/2017?incluirDetalleUbicacion=true
Authorization: Bearer {token}
```

#### Respuesta Exitosa (200 OK)

```json
{
  "success": true,
  "data": {
    "id": 2017,
    "codigoCliente": "20 DEL VALLE",
    "nombre": "COMPUTACION EN ACCION, SA DE CV",
    "rfc": "VVA160219TB1",
    "telefono": "8250300",
    "email": "contacto@compuaccion.com",
    "ubicacion": "LAUREANO RONCAL SUR 611\nZONA CENTRO\nVictoria de Durango, Durango",
    "estado": "Durango",
    "ubicacionDetalle": {
      "calle": "LAUREANO RONCAL SUR",
      "numeroExterior": "611",
      "numeroInterior": "",
      "colonia": "ZONA CENTRO",
      "codigoPostal": "34000",
      "ciudad": "Victoria de Durango",
      "municipio": "Durango",
      "estado": "Durango",
      "pais": "MEXICO",
      "telefono1": "8250300",
      "telefono2": "",
      "telefonoCompleto": "8250300"
      }
  },
  "message": "Cliente encontrado exitosamente"
}
```

#### Respuesta Cliente No Encontrado (404 Not Found)

```json
{
  "success": false,
  "message": "Cliente con ID 9999 no encontrado"
}
```

### 3. Estadísticas de Clientes

**GET** `/api/AdmClientes/stats`

Obtiene estadísticas generales de clientes.

#### Ejemplo de Petición

```http
GET /api/AdmClientes/stats
Authorization: Bearer {token}
```

#### Respuesta Exitosa (200 OK)

```json
{
  "success": true,
  "data": {
    "totalClientes": 2500,
    "clientesActivos": 2200,
    "clientesInactivos": 300,
    "porcentajeActivos": 88.00
  },
  "message": "Estadísticas obtenidas exitosamente"
}
```

## 🔑 Autenticación

Todas las APIs requieren autenticación JWT con roles `ADMINISTRACION` o `RECEPCION`.

```http
Authorization: Bearer {jwt_token}
```

O cookie HttpOnly:
```
Cookie: authToken={jwt_token}
```

## 📊 Estructura de Datos

### AdmClienteConDomicilioResponseDto

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | int | ID único del cliente (CIDCLIENTEPROVEEDOR) |
| `codigoCliente` | string | Código del cliente |
| `nombre` | string | Razón social del cliente |
| `rfc` | string | RFC (muestra "Sin RFC" si está vacío) |
| `telefono` | string | Primer teléfono disponible (domicilio > cliente) |
| `email` | string | Primer email disponible (domicilio > cliente.email1 > cliente.email2) |
| `ubicacion` | string | Dirección formateada multi-línea |
| `estado` | string | Estado del domicilio |
| `ubicacionDetalle` | object | Detalle completo de ubicación (opcional) |

### UbicacionDetalleDto

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `calle` | string | Nombre de la calle |
| `numeroExterior` | string | Número exterior |
| `numeroInterior` | string | Número interior |
| `colonia` | string | Colonia |
| `codigoPostal` | string | Código postal |
| `ciudad` | string | Ciudad |
| `municipio` | string | Municipio |
| `estado` | string | Estado |
| `pais` | string | País |
| `telefono1` | string | Teléfono 1 del domicilio |
| `telefono2` | string | Teléfono 2 del domicilio |
| `telefonoCompleto` | string | Teléfonos concatenados |

## 🔍 Relación de Tablas

```sql
-- Relación principal
admClientes.CIDCLIENTEPROVEEDOR = admDomicilios.CIDCATALOGO
                                  AND admDomicilios.CTIPOCATALOGO = 1  -- 1 = Cliente

-- Tipos de dirección
-- CTIPODIRECCION:
--   1 = Fiscal (default)
--   2 = Envío
--   3 = Otros
```

## 📝 Ejemplos de Uso

### Ejemplo 1: Buscar todos los clientes activos

```http
GET /api/AdmClientes/search?estatus=1&numeroPagina=1&tamanoPagina=50
```

### Ejemplo 2: Buscar por RFC

```http
GET /api/AdmClientes/search?rfc=VVA160219
```

### Ejemplo 3: Buscar por estado y ciudad

```http
GET /api/AdmClientes/search?estado=Durango&ciudad=Victoria
```

### Ejemplo 4: Buscar clientes sin detalle de ubicación

```http
GET /api/AdmClientes/search?incluirDetalleUbicacion=false&tamanoPagina=100
```

### Ejemplo 5: Búsqueda combinada

```http
GET /api/AdmClientes/search?razonSocial=ACCION&estado=Durango&estatus=1&numeroPagina=1
```

## ⚡ Optimizaciones Implementadas

1. **AsNoTracking**: Todas las consultas usan `.AsNoTracking()` para mejor rendimiento
2. **Evitar OPENJSON**: Se itera con `foreach` en lugar de usar `Contains()` para compatibilidad con SQL Server antiguo
3. **Carga selectiva**: Solo carga domicilios cuando se solicitan filtros de ubicación
4. **Prioridad de datos**: Telefono/Email priorizan datos del domicilio sobre datos del cliente
5. **Paginación eficiente**: Límite máximo de 100 registros por página

## 🚨 Códigos de Respuesta

| Código | Descripción |
|--------|-------------|
| 200 | Petición exitosa |
| 400 | Parámetros inválidos (ej: página < 1, tamaño > 100) |
| 404 | Cliente no encontrado (solo en GET /{id}) |
| 401 | No autenticado |
| 403 | Sin permisos (requiere rol ADMINISTRACION o RECEPCION) |
| 500 | Error interno del servidor |

## 📌 Notas Importantes

1. La tabla `admClientes` contiene TANTO clientes como proveedores. Esta API filtra automáticamente solo clientes.
2. Los domicilios se asocian mediante `CTIPOCATALOGO = 1` (Cliente).
3. Si un cliente tiene múltiples domicilios, se prioriza el domicilio marcado como predeterminado (`CPREDETERMINADO = 1`).
4. El campo `ubicacion` es una cadena formateada multi-línea para fácil visualización.
5. El campo `ubicacionDetalle` es un objeto estructurado para procesamiento programático.
6. Los filtros de búsqueda son **case-insensitive** y admiten búsqueda parcial.

## 🔧 Implementación Técnica

- **Backend**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Base de datos**: SQL Server (adCABS2016 legacy)
- **Patrón**: Repository-Service-Controller
- **Logging**: Serilog con emojis para fácil identificación
- **Autenticación**: JWT Bearer Token

## 📦 Archivos Relacionados

```
back_cabs/
├── CRM/
│   ├── models/legacy/
│   │   ├── AdmCliente.cs               # Modelo de cliente
│   │   └── AdmDomicilio.cs             # Modelo de domicilio
│   ├── DTOs/Legacy/
│   │   ├── AdmClienteConDomicilioResponseDto.cs
│   │   └── AdmClienteFilterDto.cs
│   ├── Interfaces/Legacy/
│   │   ├── IAdmClienteRepository.cs
│   │   └── IAdmClienteService.cs
│   ├── repositories/Legacy/
│   │   └── AdmClienteRepository.cs     # Lógica de acceso a datos
│   ├── services/Legacy/
│   │   └── AdmClienteService.cs        # Lógica de negocio
│   ├── controllers/legacy/
│   │   └── AdmClientesController.cs    # Endpoints REST
│   └── contexts/
│       └── LegacyCompacContext.cs      # DbContext con DbSets
```
