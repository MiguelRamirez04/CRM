-- =====================================================
-- SCRIPT COMPLETO: Tablas Actualizadas con Índices
-- Fecha: 2025-12-10
-- Descripción: CREATE TABLE completo para finance_gastos_viaticos
--              y fleet_vehiculos con todos los índices
-- =====================================================

USE CABS_Pruebas;
GO

-- =====================================================
-- TABLA: finance_gastos_viaticos (ACTUALIZADA)
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[finance_gastos_viaticos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[finance_gastos_viaticos](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [orden_id] [int] NULL,                      -- ✅ NULLABLE (viáticos independientes)
        [vehiculo_id] [int] NULL,                   -- ✅ NUEVO (tracking de vehículo)
        [tiene_factura] [bit] NOT NULL,
        [descripcion] [nvarchar](max) NULL,
        [proveedor_nombre] [varchar](200) NULL,
        [fecha] [date] NOT NULL,
        [km_recorridos] [int] NULL,
        [gastos] [varchar](200) NOT NULL,
        [monto_total] [decimal](12, 2) NOT NULL,
        [lugar_destino] [varchar](200) NULL,
        PRIMARY KEY CLUSTERED ([id] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
        ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

    PRINT '✅ Tabla finance_gastos_viaticos creada';
END
ELSE
    PRINT '⏭️  Tabla finance_gastos_viaticos ya existe';
GO

-- Default para tiene_factura
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_gastos_viaticos_tiene_factura')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos] 
    ADD CONSTRAINT [DF_gastos_viaticos_tiene_factura] DEFAULT ((0)) FOR [tiene_factura];
    PRINT '✅ Default para tiene_factura agregado';
END
GO

-- FK a ops_ordenes_trabajo (nullable)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_orden')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    WITH NOCHECK ADD CONSTRAINT [FK_gastos_viaticos_orden] 
    FOREIGN KEY([orden_id])
    REFERENCES [dbo].[ops_ordenes_trabajo] ([id])
    ON DELETE CASCADE;
    
    ALTER TABLE [dbo].[finance_gastos_viaticos] 
    NOCHECK CONSTRAINT [FK_gastos_viaticos_orden];
    
    PRINT '✅ FK_gastos_viaticos_orden creada';
END
GO

-- FK a fleet_vehiculos
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_gastos_viaticos_vehiculo')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    ADD CONSTRAINT [FK_gastos_viaticos_vehiculo] 
    FOREIGN KEY([vehiculo_id])
    REFERENCES [dbo].[fleet_vehiculos] ([id]);
    
    PRINT '✅ FK_gastos_viaticos_vehiculo creada';
END
GO

-- Check constraint para km_recorridos
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_viatico_km')
BEGIN
    ALTER TABLE [dbo].[finance_gastos_viaticos]
    WITH NOCHECK ADD CONSTRAINT [CK_viatico_km] 
    CHECK (([km_recorridos] IS NULL OR [km_recorridos]>=(0)));
    
    ALTER TABLE [dbo].[finance_gastos_viaticos] 
    NOCHECK CONSTRAINT [CK_viatico_km];
    
    PRINT '✅ CK_viatico_km creado';
END
GO

-- =====================================================
-- TABLA: fleet_vehiculos (ACTUALIZADA)
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fleet_vehiculos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[fleet_vehiculos](
        [id] [int] IDENTITY(1,1) NOT NULL,
        [tipo_vehiculo] [varchar](50) NULL,
        [transmision] [varchar](20) NULL,
        [es_de_empresa] [bit] NOT NULL,
        [placas] [varchar](20) NULL,
        [activo] [bit] NOT NULL,
        [disponible] [bit] NOT NULL,              -- ✅ NUEVO (control de disponibilidad)
        [observaciones] [nvarchar](max) NULL,
        [nombre_vehiculo] [varchar](100) NOT NULL,
        [kilometraje] [int] NULL,
        PRIMARY KEY CLUSTERED ([id] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
        ON [PRIMARY],
        UNIQUE NONCLUSTERED ([placas] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
        ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

    PRINT '✅ Tabla fleet_vehiculos creada';
END
ELSE
    PRINT '⏭️  Tabla fleet_vehiculos ya existe';
GO

-- Defaults para fleet_vehiculos
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_vehiculos_es_de_empresa')
BEGIN
    ALTER TABLE [dbo].[fleet_vehiculos] 
    ADD CONSTRAINT [DF_vehiculos_es_de_empresa] DEFAULT ((1)) FOR [es_de_empresa];
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_vehiculos_activo')
BEGIN
    ALTER TABLE [dbo].[fleet_vehiculos] 
    ADD CONSTRAINT [DF_vehiculos_activo] DEFAULT ((1)) FOR [activo];
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_vehiculos_disponible')
BEGIN
    ALTER TABLE [dbo].[fleet_vehiculos] 
    ADD CONSTRAINT [DF_vehiculos_disponible] DEFAULT ((1)) FOR [disponible];
    PRINT '✅ Default para disponible agregado';
END
GO

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_vehiculos_nombre')
BEGIN
    ALTER TABLE [dbo].[fleet_vehiculos] 
    ADD CONSTRAINT [DF_vehiculos_nombre] DEFAULT ('') FOR [nombre_vehiculo];
END
GO

-- =====================================================
-- ÍNDICES PARA finance_gastos_viaticos
-- =====================================================

PRINT '';
PRINT '🔍 Creando índices para finance_gastos_viaticos...';
GO

-- Índice para búsquedas por VehiculoId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_VehiculoId' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_VehiculoId
    ON finance_gastos_viaticos(vehiculo_id)
    WHERE vehiculo_id IS NOT NULL
    INCLUDE (fecha, monto_total, lugar_destino, km_recorridos);
    PRINT '  ✅ IX_GastosViaticos_VehiculoId';
END
GO

-- Índice para búsquedas por OrdenId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_OrdenId' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_OrdenId
    ON finance_gastos_viaticos(orden_id)
    WHERE orden_id IS NOT NULL
    INCLUDE (fecha, monto_total, tiene_factura, lugar_destino);
    PRINT '  ✅ IX_GastosViaticos_OrdenId';
END
GO

-- Índice para búsquedas por Fecha
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_Fecha' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_Fecha
    ON finance_gastos_viaticos(fecha DESC)
    INCLUDE (orden_id, vehiculo_id, monto_total, tiene_factura);
    PRINT '  ✅ IX_GastosViaticos_Fecha';
END
GO

-- Índice compuesto para rango de fechas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_FechaRango' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_FechaRango
    ON finance_gastos_viaticos(fecha, orden_id)
    INCLUDE (monto_total, lugar_destino, tiene_factura, km_recorridos);
    PRINT '  ✅ IX_GastosViaticos_FechaRango';
END
GO

-- Índice para búsquedas por factura
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_TieneFactura' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_TieneFactura
    ON finance_gastos_viaticos(tiene_factura, fecha DESC)
    INCLUDE (orden_id, monto_total, proveedor_nombre, descripcion);
    PRINT '  ✅ IX_GastosViaticos_TieneFactura';
END
GO

-- Índice para búsquedas por lugar destino
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GastosViaticos_LugarDestino' AND object_id = OBJECT_ID('finance_gastos_viaticos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_GastosViaticos_LugarDestino
    ON finance_gastos_viaticos(lugar_destino, fecha DESC)
    WHERE lugar_destino IS NOT NULL
    INCLUDE (monto_total, km_recorridos, orden_id);
    PRINT '  ✅ IX_GastosViaticos_LugarDestino';
END
GO

-- =====================================================
-- ÍNDICES PARA fleet_vehiculos
-- =====================================================

PRINT '';
PRINT '🔍 Creando índices para fleet_vehiculos...';
GO

-- Índice para vehículos disponibles
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Disponible' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Disponible
    ON fleet_vehiculos(disponible, activo)
    WHERE disponible = 1 AND activo = 1
    INCLUDE (nombre_vehiculo, placas, tipo_vehiculo, transmision);
    PRINT '  ✅ IX_Vehiculos_Disponible';
END
GO

-- Índice para búsquedas por placas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Placas' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Placas
    ON fleet_vehiculos(placas)
    WHERE placas IS NOT NULL
    INCLUDE (nombre_vehiculo, activo, disponible);
    PRINT '  ✅ IX_Vehiculos_Placas';
END
GO

-- Índice para vehículos activos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Vehiculos_Activo' AND object_id = OBJECT_ID('fleet_vehiculos'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Vehiculos_Activo
    ON fleet_vehiculos(activo, disponible)
    INCLUDE (nombre_vehiculo, placas, tipo_vehiculo);
    PRINT '  ✅ IX_Vehiculos_Activo';
END
GO

PRINT '';
PRINT '✅ ¡Script completado exitosamente!';
PRINT '';
PRINT '📊 Resumen:';
PRINT '   - finance_gastos_viaticos: 6 índices';
PRINT '   - fleet_vehiculos: 3 índices';
PRINT '   TOTAL: 9 índices';
PRINT '';
GO
