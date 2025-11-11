# 📦 MODELOS LEGACY Y CATALOG

## 📋 Descripción General

Este módulo contiene las entidades C# que representan el sistema de catálogos migrados desde el sistema legacy Compaq Administrativo (adCABS2016).

La arquitectura se divide en dos esquemas principales:
- **legacy**: Tablas de referencia (solo IDs) que mantienen integridad referencial con el sistema antiguo
- **catalog**: Tablas con datos completos migrados más información local

## 🏗️ Arquitectura de Datos

### Esquema: **legacy** (Tablas Puente)

Estas tablas solo contienen IDs del sistema legacy y sirven como puente referencial:

| Tabla | Descripción | PK Type |
|-------|-------------|---------|
| `monedas_ref` | Referencias de monedas | INT |
| `agentes_ref` | Referencias de agentes/vendedores | INT |
| `almacenes_ref` | Referencias de almacenes/bodegas | INT |
| `productos_ref` | Referencias de productos/servicios | INT |
| `documentos_modelo_ref` | Referencias de tipos de documentos | INT |
| `conceptos_ref` | Referencias de conceptos de operación | INT |
| `numeros_serie_ref` | Referencias de números de serie | INT |
| `documentos_ref` | Referencias de documentos comerciales | INT |
| `movimientos_ref` | Referencias de movimientos/partidas | INT |
| `clientes_ref` | Referencias de clientes (migración previa) | BIGINT |

### Esquema: **catalog** (Datos Locales)

Estas tablas contienen datos completos migrados y nuevos:

| Tabla | Descripción | Relaciones Legacy |
|-------|-------------|-------------------|
| `monedas` | Catálogo de monedas | ↔ `monedas_ref` |
| `agentes` | Catálogo de agentes/vendedores | ↔ `agentes_ref` |
| `almacenes` | Catálogo de almacenes/bodegas | ↔ `almacenes_ref` |
| `productos` | Catálogo de productos/servicios | ↔ `productos_ref` |
| `documentos_modelo` | Tipos de documentos | ↔ `documentos_modelo_ref` |
| `conceptos` | Conceptos de operación | ↔ `conceptos_ref`, `documentos_modelo_ref` |
| `numeros_serie` | Control de números de serie | ↔ `numeros_serie_ref`, `productos_ref`, `almacenes_ref` |
| `documentos` | Documentos comerciales | ↔ `documentos_ref`, `documentos_modelo_ref`, `conceptos_ref`, `clientes_ref`, `agentes_ref` |
| `movimientos` | Partidas de documentos | ↔ `movimientos_ref`, `documentos_ref`, `productos_ref`, `documentos_modelo_ref` |
| `movimientos_serie` | Relación movimientos-series | ↔ `movimientos_ref`, `numeros_serie_ref` |

## 🔗 Diagrama de Relaciones

```
┌─────────────────────────────────────────────────────────────────┐
│                    ESQUEMA: legacy (Ref)                         │
└─────────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ESQUEMA: catalog (Data)                       │
│                                                                  │
│  Moneda ──┬──> Agente ──┐                                       │
│           │             ├──> Documento ──> Movimiento ──┐       │
│  Producto ┴──> Almacén ─┘                               │       │
│           │                                             │       │
│           └──> NumeroSerie <──> MovimientoSerie <───────┘       │
└─────────────────────────────────────────────────────────────────┘
```

## 📝 Modelos Implementados

### 1. Tablas Legacy (Referencias)

#### MonedasRef.cs
- **Propósito**: Referencia a monedas del sistema legacy
- **PK**: `LegacyMonedaId` (INT)
- **Relaciones**: → `catalog.monedas`

#### AgentesRef.cs
- **Propósito**: Referencia a agentes/vendedores
- **PK**: `LegacyAgenteId` (INT)
- **Relaciones**: → `catalog.agentes`, `catalog.documentos`

#### AlmacenesRef.cs
- **Propósito**: Referencia a almacenes/bodegas
- **PK**: `LegacyAlmacenId` (INT)
- **Relaciones**: → `catalog.almacenes`, `catalog.numeros_serie`

#### ProductosRef.cs
- **Propósito**: Referencia a productos/servicios
- **PK**: `LegacyProductoId` (INT)
- **Relaciones**: → `catalog.productos`, `catalog.numeros_serie`, `catalog.movimientos`

#### DocumentosModeloRef.cs
- **Propósito**: Referencia a tipos de documentos
- **PK**: `LegacyDocumentoModeloId` (INT)
- **Relaciones**: → `catalog.documentos_modelo`, `catalog.conceptos`, `catalog.movimientos`

#### ConceptosRef.cs
- **Propósito**: Referencia a conceptos de operación
- **PK**: `LegacyConceptoId` (INT)
- **Relaciones**: → `catalog.conceptos`, `catalog.documentos`

#### NumerosSerieRef.cs
- **Propósito**: Referencia a números de serie
- **PK**: `LegacySerieId` (INT)
- **Relaciones**: → `catalog.numeros_serie`, `catalog.movimientos_serie`

#### DocumentosRef.cs
- **Propósito**: Referencia a documentos comerciales
- **PK**: `LegacyDocumentoId` (INT)
- **Relaciones**: → `catalog.documentos`, `catalog.movimientos`

#### MovimientosRef.cs
- **Propósito**: Referencia a movimientos/partidas
- **PK**: `LegacyMovimientoId` (INT)
- **Relaciones**: → `catalog.movimientos`, `catalog.movimientos_serie`

#### ClientesRef.cs
- **Propósito**: Referencia a clientes (ya existente)
- **PK**: `LegacyClientId` (BIGINT)
- **Relaciones**: → `catalog.clientes`, `catalog.documentos`

### 2. Tablas Catalog (Datos)

#### Moneda.cs (catalog.monedas)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyMonedaId`: FK a `legacy.monedas_ref`
- `NombreMoneda`: Nombre (ej: "Peso Mexicano")
- `SimboloMoneda`: Símbolo ($, USD)
- `ClaveSat`: Clave SAT para facturación
- `Decimales`: Precisión decimal
- `Activo`: Estado

**Uso:** Configuración de monedas en transacciones

#### Agente.cs (catalog.agentes)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyAgenteId`: FK a `legacy.agentes_ref`
- `CodigoAgente`: Código único
- `NombreAgente`: Nombre completo
- `TipoAgente`: Tipo (vendedor, cobrador)
- `FechaAlta`: Fecha de alta
- `Activo`: Estado

**Uso:** Asignación de vendedores/cobradores a documentos

#### Almacen.cs (catalog.almacenes)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyAlmacenId`: FK a `legacy.almacenes_ref`
- `CodigoAlmacen`: Código único
- `NombreAlmacen`: Nombre descriptivo
- `Clasificacion1`: Categoría
- `FechaAlta`: Fecha de alta
- `Activo`: Estado

**Uso:** Control de inventarios por ubicación

#### Producto.cs (catalog.productos)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyProductoId`: FK a `legacy.productos_ref`
- `CodigoProducto`: SKU
- `NombreProducto`: Nombre descriptivo
- `TipoProducto`: Tipo (producto, servicio, paquete)
- `DescripcionProducto`: Descripción detallada
- `Precio1`: Precio principal
- `ClaveSat`: Clave SAT
- `MetodoCosteo`: Método (Promedio, PEPS, UEPS)
- `Activo`: Estado

**Uso:** Catálogo de productos/servicios comercializables

#### DocumentoModelo.cs (catalog.documentos_modelo)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyDocumentoModeloId`: FK a `legacy.documentos_modelo_ref`
- `Descripcion`: Tipo de documento
- `Naturaleza`: Cargo/Abono
- `AfectaExistencia`: Cómo afecta inventario
- `NoFolio`: Folio actual
- `Activo`: Estado

**Uso:** Definición de tipos de documentos (factura, remisión, etc.)

#### Concepto.cs (catalog.conceptos)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyConceptoId`: FK a `legacy.conceptos_ref`
- `LegacyDocumentoModeloId`: FK a `legacy.documentos_modelo_ref`
- `CodigoConcepto`: Código único
- `NombreConcepto`: Nombre descriptivo
- `Naturaleza`: Cargo/Abono
- `TipoFolio`: Tipo de foliación
- `CreaCliente`: Si crea cliente automáticamente
- `Activo`: Estado

**Uso:** Operaciones específicas dentro de documentos

#### NumeroSerie.cs (catalog.numeros_serie)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacySerieId`: FK a `legacy.numeros_serie_ref`
- `LegacyProductoId`: FK a `legacy.productos_ref`
- `LegacyAlmacenId`: FK a `legacy.almacenes_ref`
- `NumeroSerieValor`: Número de serie único
- `Estado`: Estado actual
- `EstadoAnterior`: Estado previo
- `Costo`: Costo específico
- `FechaTimestamp`: Última modificación
- `Activo`: Estado

**Uso:** Control individual de productos serializados

#### Documento.cs (catalog.documentos)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyDocumentoId`: FK a `legacy.documentos_ref`
- `LegacyDocumentoModeloId`: FK a `legacy.documentos_modelo_ref`
- `LegacyConceptoId`: FK a `legacy.conceptos_ref`
- `LegacyClienteId`: FK a `legacy.clientes_ref`
- `LegacyAgenteId`: FK a `legacy.agentes_ref`
- `SerieDocumento`: Serie
- `Folio`: Número de folio
- `Fecha`: Fecha de emisión
- `RazonSocial`: Cliente
- `Rfc`: RFC del cliente
- `Neto`, `Impuesto1`, `Total`, `Pendiente`: Importes
- `Cancelado`, `Afectado`, `Impreso`: Estados
- `Activo`: Estado

**Uso:** Facturas, pedidos, remisiones, etc.

#### Movimiento.cs (catalog.movimientos)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyMovimientoId`: FK a `legacy.movimientos_ref`
- `LegacyDocumentoId`: FK a `legacy.documentos_ref`
- `LegacyProductoId`: FK a `legacy.productos_ref`
- `LegacyDocumentoModeloId`: FK a `legacy.documentos_modelo_ref`
- `NumeroMovimiento`: Consecutivo
- `Unidades`: Cantidad
- `PrecioCapturado`: Precio unitario
- `CostoEspecifico`: Costo
- `Neto`, `Impuesto1`, `Descuento1`, `Total`: Importes
- `AfectaExistencia`, `AfectadoSaldos`, `AfectadoInventario`: Control
- `Activo`: Estado

**Uso:** Partidas/líneas de detalle en documentos

#### MovimientoSerie.cs (catalog.movimientos_serie)
**Propiedades principales:**
- `Id`: PK autoincremental
- `LegacyMovimientoId`: FK a `legacy.movimientos_ref`
- `LegacySerieId`: FK a `legacy.numeros_serie_ref`
- `Fecha`: Fecha del movimiento

**Uso:** Rastreo de números de serie en movimientos

## 🛠️ Convenciones y Buenas Prácticas

### Nomenclatura
- **Tablas legacy**: `{Entidad}Ref` (ej: `MonedasRef`)
- **Tablas catalog**: `{Entidad}` (ej: `Moneda`)
- **Esquemas DB**: `legacy` y `catalog`
- **PKs**: `Id` (catalog), `Legacy{Entidad}Id` (legacy)
- **FKs**: `Legacy{Entidad}Id`

### Atributos Estándar
- `[Table("{nombre}", Schema = "legacy|catalog")]`
- `[Key]` en PKs
- `[Column("{nombre_db}")]` mapeo exacto
- `[DatabaseGenerated(DatabaseGeneratedOption.None)]` en PKs legacy
- `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` en PKs catalog
- `[MaxLength(n)]` en strings
- `[Precision(18, 4)]` en decimales
- `[ForeignKey(nameof(PropertyName))]` en navegación

### Propiedades Comunes
Todas las entidades catalog tienen:
- `Id` (PK autoincremental)
- `Legacy{Entity}Id` (FK nullable al legacy)
- `Activo` (bool, default = true)

## 📚 Uso y Ejemplos

### Ejemplo 1: Consultar Productos Activos
```csharp
var productos = await context.Productos
    .Where(p => p.Activo)
    .Include(p => p.ProductosRef)
    .ToListAsync();
```

### Ejemplo 2: Crear un Documento con Movimientos
```csharp
var documento = new Documento
{
    LegacyClienteId = clienteId,
    LegacyAgenteId = agenteId,
    SerieDocumento = "A",
    Folio = 1001,
    Fecha = DateTime.Now,
    Activo = true
};

var movimiento = new Movimiento
{
    LegacyDocumentoId = documento.LegacyDocumentoId,
    LegacyProductoId = productoId,
    Unidades = 5,
    PrecioCapturado = 100.00m,
    Total = 500.00m,
    Activo = true
};
```

### Ejemplo 3: Rastrear Números de Serie
```csharp
var serie = await context.NumerosSerie
    .Where(ns => ns.NumeroSerieValor == "ABC123")
    .Include(ns => ns.ProductosRef)
    .Include(ns => ns.AlmacenesRef)
    .FirstOrDefaultAsync();
```

## ⚠️ Consideraciones Importantes

1. **Integridad Referencial**: Las FKs a legacy son opcionales (nullable) para permitir datos nuevos sin origen legacy

2. **Migración**: Los IDs legacy se mantienen para sincronización con sistema antiguo

3. **Soft Delete**: Se usa el campo `Activo` en vez de eliminar físicamente

4. **Precisión Decimal**: Los importes usan `DECIMAL(18,4)` para compatibilidad con sistema legacy

5. **Esquemas Separados**: `legacy` y `catalog` deben configurarse en DbContext

## 🔄 Próximos Pasos

- [ ] Configurar DbContext con esquemas legacy y catalog
- [ ] Crear migraciones de Entity Framework
- [ ] Implementar repositorios específicos
- [ ] Crear DTOs para APIs
- [ ] Implementar validaciones de negocio
- [ ] Documentar reglas de afectación de inventarios

---

**Autor**: Sistema CRM CABS  
**Fecha**: Noviembre 2025  
**Versión**: 1.0
