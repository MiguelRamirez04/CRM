CREATE OR ALTER VIEW dbo.VwNumerosSerieCompletos
AS
SELECT
    ns.id AS NumeroSerieId,
    ns.numero_serie AS NumeroSerie,
    ns.estado AS Estado,
    ns.estado_anterior AS EstadoAnterior,
    ns.costo AS Costo,
    ns.fecha_timestamp AS FechaTimestamp,
    ns.activo AS Activo,
    -- Datos de legacy
    leg.legacy_serie_id AS LegacySerieId,
    ns.legacy_producto_id AS LegacyProductoId,
    ns.legacy_almacen_id AS LegacyAlmacenId,
    leg_ns.CIDPRODUCTO AS ProductoLegacy,
    leg_ns.CNUMEROSERIE AS NumeroSerieLegacy,
    leg_ns.CIDALMACEN AS AlmacenLegacy,
    leg_ns.CESTADO AS EstadoLegacy,
    leg_ns.CESTADOANTERIOR AS EstadoAnteriorLegacy,
    CAST(leg_ns.CCOSTO AS DECIMAL(18,4)) AS CostoLegacy,
    leg_ns.CTIMESTAMP AS TimestampLegacy
FROM
    catalog.numeros_serie AS ns
LEFT JOIN
    legacy.numeros_serie_ref AS leg ON ns.legacy_serie_id = leg.legacy_serie_id
LEFT JOIN
    [COMPAC].[adCABS2016].[dbo].[admNumerosSerie] AS leg_ns ON leg.legacy_serie_id = leg_ns.CIDSERIE;
GO