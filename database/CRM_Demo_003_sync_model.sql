-- CRM_Demo_003_sync_model.sql
-- Completa la sincronizacion entre el modelo EF del backend y Supabase.
-- Corrige tablas nuevas con columnas que el backend espera y agrega las tablas legacy
-- que faltan para que funcionen los endpoints /api/Adm*.
--
-- Ejecutar en Supabase SQL Editor DESPUES de CRM_Demo_001_create.sql y CRM_Demo_002_seed.sql.

-- =============================================================================
-- 1. Corregir sales_cotizaciones para que coincida con el modelo EF Cotizacion
-- =============================================================================
-- El backend EF mapea Cotizacion a sales_cotizaciones con columnas snake_case.
-- La tabla original venia con prefijos 'c' del esquema legacy. Se migran los datos
-- a una estructura nueva y se reemplaza la tabla.

CREATE TABLE IF NOT EXISTS sales_cotizaciones_new (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    orden_id integer NULL,
    intake_legacy_id integer NULL,
    subtotal numeric(12,2) NOT NULL DEFAULT 0,
    impuestos_total numeric(12,2) NOT NULL DEFAULT 0,
    total numeric(12,2) NOT NULL DEFAULT 0,
    estado varchar(20) NOT NULL DEFAULT 'NUEVA',
    observaciones varchar(500) NULL,
    actualizado_en timestamp NULL,
    creado_en timestamp NOT NULL DEFAULT now(),
    validez_dias integer NULL,
    horas_capacitacion integer NULL,
    paquetes_capacitacion integer NULL,
    costo_capacitacion numeric(12,2) NULL,
    cliente varchar(255) NULL,
    rfc varchar(13) NULL,
    folio varchar(50) NULL,
    descuento numeric(12,2) NULL,
    descrpcion_servicio varchar(1000) NULL,
    telefono bigint NULL,
    correo varchar(150) NULL
);

INSERT INTO sales_cotizaciones_new (
    id, orden_id, intake_legacy_id, subtotal, impuestos_total, total,
    estado, observaciones, actualizado_en, creado_en, validez_dias,
    horas_capacitacion, paquetes_capacitacion, costo_capacitacion,
    cliente, rfc, folio, descuento, descrpcion_servicio, telefono, correo
)
SELECT
    cid_documento,
    NULL,
    NULL,
    COALESCE(cneto, 0),
    0,
    COALESCE(ctotal, 0),
    CASE ccancelado WHEN 1 THEN 'CANCELADA' ELSE 'NUEVA' END,
    cobservaciones,
    NULL,
    cfeha,
    NULL,
    NULL,
    NULL,
    NULL,
    crazon_social,
    crfc,
    NULL,
    COALESCE(cdescuentomov, 0),
    NULL,
    NULL,
    NULL
FROM sales_cotizaciones;

DROP TABLE sales_cotizaciones;

ALTER TABLE sales_cotizaciones_new RENAME TO sales_cotizaciones;

ALTER TABLE sales_cotizaciones DROP COLUMN IF EXISTS total CASCADE;
ALTER TABLE sales_cotizaciones ADD COLUMN total numeric(12,2) GENERATED ALWAYS AS (subtotal + impuestos_total) STORED;

-- =============================================================================
-- 2. Crear cotizacion_items (falta en 001)
-- =============================================================================
CREATE TABLE IF NOT EXISTS cotizacion_items (
    id integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cotizacion_id integer NOT NULL,
    producto_servicio_ref_id integer NULL,
    legacy_producto_id integer NULL,
    descripcion varchar(500) NULL,
    cantidad numeric(12,3) NOT NULL DEFAULT 0,
    precio_unitario numeric(12,2) NOT NULL DEFAULT 0,
    total_linea numeric(12,2) NULL
);

-- =============================================================================
-- 3. Agregar columnas faltantes a tablas nuevas
-- =============================================================================

-- ops_ordenes_trabajo: campos para clientes nuevos/local
ALTER TABLE ops_ordenes_trabajo ADD COLUMN IF NOT EXISTS nuevo_cliente boolean NULL;
ALTER TABLE ops_ordenes_trabajo ADD COLUMN IF NOT EXISTS nombre_cliente varchar(120) NULL;

-- fleet_vehiculos: el backend EF mapea Kilometraje a kilometraje_actual.
-- Las columnas de auditoria (creado_en, etc.) son [NotMapped] en el backend,
-- por lo que no requieren columna fisica en BD. Se eliminan las alteraciones
-- innecesarias del script anterior.
-- NOTA: NO se agrega 'kilometraje' porque EF ya usa 'kilometraje_actual'.

-- finance_gastos_viaticos: VehiculoId es [NotMapped] en el backend,
-- no requiere columna en BD. Se elimina la alteracion innecesaria.

-- ejecuciones_orden: el backend EF mapea EjecucionOrden a ops_ejecuciones_orden,
-- NO a ejecuciones_orden. Se eliminan las alteraciones incorrectas sobre
-- ejecuciones_orden (esa tabla es otro contexto/legacy).

-- reparaciones: NombreCliente y Telefono son [NotMapped] en el backend,
-- no requieren columnas en BD. Se eliminan las alteraciones innecesarias.

-- =============================================================================
-- 4. Crear tablas legacy necesarias para los endpoints /api/Adm*
-- =============================================================================

-- admClientes
CREATE TABLE IF NOT EXISTS "admClientes" (
    "CIDCLIENTEPROVEEDOR" integer PRIMARY KEY,
    "CCODIGOCLIENTE" varchar(30) NOT NULL DEFAULT '',
    "CRAZONSOCIAL" varchar(60) NOT NULL DEFAULT '',
    "CFECHAALTA" timestamp NOT NULL DEFAULT now(),
    "CRFC" varchar(20) NOT NULL DEFAULT '',
    "CCURP" varchar(20) NOT NULL DEFAULT '',
    "CDENCOMERCIAL" varchar(50) NOT NULL DEFAULT '',
    "CREPLEGAL" varchar(50) NOT NULL DEFAULT '',
    "CIDMONEDA" integer NOT NULL DEFAULT 0,
    "CLISTAPRECIOCLIENTE" integer NOT NULL DEFAULT 0,
    "CDESCUENTODOCTO" double precision NOT NULL DEFAULT 0,
    "CDESCUENTOMOVTO" double precision NOT NULL DEFAULT 0,
    "CBANVENTACREDITO" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFCLIENTE1" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFCLIENTE2" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFCLIENTE3" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFCLIENTE4" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFCLIENTE5" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFCLIENTE6" integer NOT NULL DEFAULT 0,
    "CTIPOCLIENTE" integer NOT NULL DEFAULT 0,
    "CESTATUS" integer NOT NULL DEFAULT 0,
    "CFECHABAJA" timestamp NOT NULL DEFAULT now(),
    "CFECHAULTIMAREVISION" timestamp NOT NULL DEFAULT now(),
    "CLIMITECREDITOCLIENTE" double precision NOT NULL DEFAULT 0,
    "CDIASCREDITOCLIENTE" integer NOT NULL DEFAULT 0,
    "CBANEXCEDERCREDITO" integer NOT NULL DEFAULT 0,
    "CDESCUENTOPRONTOPAGO" double precision NOT NULL DEFAULT 0,
    "CDIASPRONTOPAGO" integer NOT NULL DEFAULT 0,
    "CINTERESMORATORIO" double precision NOT NULL DEFAULT 0,
    "CDIAPAGO" integer NOT NULL DEFAULT 0,
    "CDIASREVISION" integer NOT NULL DEFAULT 0,
    "CMENSAJERIA" varchar(20) NOT NULL DEFAULT '',
    "CCUENTAMENSAJERIA" varchar(60) NOT NULL DEFAULT '',
    "CDIASEMBARQUECLIENTE" integer NOT NULL DEFAULT 0,
    "CIDALMACEN" integer NOT NULL DEFAULT 0,
    "CIDAGENTEVENTA" integer NOT NULL DEFAULT 0,
    "CIDAGENTECOBRO" integer NOT NULL DEFAULT 0,
    "CRESTRICCIONAGENTE" integer NOT NULL DEFAULT 0,
    "CIMPUESTO1" double precision NOT NULL DEFAULT 0,
    "CIMPUESTO2" double precision NOT NULL DEFAULT 0,
    "CIMPUESTO3" double precision NOT NULL DEFAULT 0,
    "CRETENCIONCLIENTE1" double precision NOT NULL DEFAULT 0,
    "CRETENCIONCLIENTE2" double precision NOT NULL DEFAULT 0,
    "CIDVALORCLASIFPROVEEDOR1" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFPROVEEDOR2" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFPROVEEDOR3" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFPROVEEDOR4" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFPROVEEDOR5" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFPROVEEDOR6" integer NOT NULL DEFAULT 0,
    "CLIMITECREDITOPROVEEDOR" double precision NOT NULL DEFAULT 0,
    "CDIASCREDITOPROVEEDOR" integer NOT NULL DEFAULT 0,
    "CTIEMPOENTREGA" integer NOT NULL DEFAULT 0,
    "CDIASEMBARQUEPROVEEDOR" integer NOT NULL DEFAULT 0,
    "CIMPUESTOPROVEEDOR1" double precision NOT NULL DEFAULT 0,
    "CIMPUESTOPROVEEDOR2" double precision NOT NULL DEFAULT 0,
    "CIMPUESTOPROVEEDOR3" double precision NOT NULL DEFAULT 0,
    "CRETENCIONPROVEEDOR1" double precision NOT NULL DEFAULT 0,
    "CRETENCIONPROVEEDOR2" double precision NOT NULL DEFAULT 0,
    "CBANINTERESMORATORIO" integer NOT NULL DEFAULT 0,
    "CCOMVENTAEXCEPCLIENTE" double precision NOT NULL DEFAULT 0,
    "CCOMCOBROEXCEPCLIENTE" double precision NOT NULL DEFAULT 0,
    "CBANPRODUCTOCONSIGNACION" integer NOT NULL DEFAULT 0,
    "CSEGCONTCLIENTE1" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTCLIENTE2" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTCLIENTE3" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTCLIENTE4" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTCLIENTE5" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTCLIENTE6" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTCLIENTE7" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPROVEEDOR1" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPROVEEDOR2" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPROVEEDOR3" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPROVEEDOR4" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPROVEEDOR5" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPROVEEDOR6" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPROVEEDOR7" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA1" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA2" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA3" varchar(50) NOT NULL DEFAULT '',
    "CFECHAEXTRA" timestamp NOT NULL DEFAULT now(),
    "CIMPORTEEXTRA1" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA2" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA3" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA4" double precision NOT NULL DEFAULT 0,
    "CBANDOMICILIO" integer NOT NULL DEFAULT 0,
    "CBANCREDITOYCOBRANZA" integer NOT NULL DEFAULT 0,
    "CBANENVIO" integer NOT NULL DEFAULT 0,
    "CBANAGENTE" integer NOT NULL DEFAULT 0,
    "CBANIMPUESTO" integer NOT NULL DEFAULT 0,
    "CBANPRECIO" integer NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT '',
    "CFACTERC01" integer NOT NULL DEFAULT 0,
    "CCOMVENTA" double precision NOT NULL DEFAULT 0,
    "CCOMCOBRO" double precision NOT NULL DEFAULT 0,
    "CIDMONEDA2" integer NOT NULL DEFAULT 0,
    "CEMAIL1" varchar(60) NOT NULL DEFAULT '',
    "CEMAIL2" varchar(60) NOT NULL DEFAULT '',
    "CEMAIL3" varchar(60) NOT NULL DEFAULT '',
    "CTIPOENTRE" integer NOT NULL DEFAULT 0,
    "CCONCTEEMA" integer NOT NULL DEFAULT 0,
    "CFTOADDEND" integer NOT NULL DEFAULT 0,
    "CIDCERTCTE" integer NOT NULL DEFAULT 0,
    "CENCRIPENT" integer NOT NULL DEFAULT 0,
    "CBANCFD" integer NOT NULL DEFAULT 0,
    "CTEXTOEXTRA4" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA5" varchar(50) NOT NULL DEFAULT '',
    "CIMPORTEEXTRA5" double precision NOT NULL DEFAULT 0,
    "CIDADDENDA" integer NOT NULL DEFAULT 0,
    "CCODPROVCO" varchar(60) NOT NULL DEFAULT '',
    "CENVACUSE" integer NOT NULL DEFAULT 0,
    "CCON1NOM" varchar(60) NOT NULL DEFAULT '',
    "CCON1TEL" varchar(15) NOT NULL DEFAULT '',
    "CQUITABLAN" integer NOT NULL DEFAULT 0,
    "CFMTOENTRE" integer NOT NULL DEFAULT 0,
    "CIDCOMPLEM" integer NOT NULL DEFAULT 0,
    "CDESGLOSAI2" integer NOT NULL DEFAULT 0,
    "CLIMDOCTOS" integer NOT NULL DEFAULT 0,
    "CSITIOFTP" varchar(60) NOT NULL DEFAULT '',
    "CUSRFTP" varchar(60) NOT NULL DEFAULT '',
    "CMETODOPAG" varchar(100) NOT NULL DEFAULT '',
    "CNUMCTAPAG" varchar(100) NOT NULL DEFAULT '',
    "CIDCUENTA" integer NOT NULL DEFAULT 0,
    "CUSOCFDI" varchar(30) NOT NULL DEFAULT '',
    "CREGIMFISC" varchar(20) NOT NULL DEFAULT '',
    "CWHATSAPP" varchar(15) NOT NULL DEFAULT ''
);

-- admProductos
CREATE TABLE IF NOT EXISTS "admProductos" (
    "CIDPRODUCTO" integer PRIMARY KEY,
    "CCODIGOPRODUCTO" varchar(30) NOT NULL DEFAULT '',
    "CNOMBREPRODUCTO" varchar(60) NOT NULL DEFAULT '',
    "CTIPOPRODUCTO" integer NOT NULL DEFAULT 0,
    "CFECHAALTAPRODUCTO" timestamp NOT NULL DEFAULT now(),
    "CCONTROLEXISTENCIA" integer NOT NULL DEFAULT 0,
    "CIDFOTOPRODUCTO" integer NOT NULL DEFAULT 0,
    "CDESCRIPCIONPRODUCTO" text NULL,
    "CMETODOCOSTEO" integer NOT NULL DEFAULT 0,
    "CPESOPRODUCTO" double precision NOT NULL DEFAULT 0,
    "CCOMVENTAEXCEPPRODUCTO" double precision NOT NULL DEFAULT 0,
    "CCOMCOBROEXCEPPRODUCTO" double precision NOT NULL DEFAULT 0,
    "CCOSTOESTANDAR" double precision NOT NULL DEFAULT 0,
    "CMARGENUTILIDAD" double precision NOT NULL DEFAULT 0,
    "CSTATUSPRODUCTO" integer NOT NULL DEFAULT 0,
    "CIDUNIDADBASE" integer NOT NULL DEFAULT 0,
    "CIDUNIDADNOCONVERTIBLE" integer NOT NULL DEFAULT 0,
    "CFECHABAJA" timestamp NOT NULL DEFAULT now(),
    "CIMPUESTO1" double precision NOT NULL DEFAULT 0,
    "CIMPUESTO2" double precision NOT NULL DEFAULT 0,
    "CIMPUESTO3" double precision NOT NULL DEFAULT 0,
    "CRETENCION1" double precision NOT NULL DEFAULT 0,
    "CRETENCION2" double precision NOT NULL DEFAULT 0,
    "CIDPADRECARACTERISTICA1" integer NOT NULL DEFAULT 0,
    "CIDPADRECARACTERISTICA2" integer NOT NULL DEFAULT 0,
    "CIDPADRECARACTERISTICA3" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION1" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION2" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION3" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION4" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION5" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION6" integer NOT NULL DEFAULT 0,
    "CSEGCONTPRODUCTO1" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPRODUCTO2" varchar(50) NOT NULL DEFAULT '',
    "CSEGCONTPRODUCTO3" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA1" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA2" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA3" varchar(50) NOT NULL DEFAULT '',
    "CFECHAEXTRA" timestamp NOT NULL DEFAULT now(),
    "CIMPORTEEXTRA1" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA2" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA3" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA4" double precision NOT NULL DEFAULT 0,
    "CPRECIO1" double precision NOT NULL DEFAULT 0,
    "CPRECIO2" double precision NOT NULL DEFAULT 0,
    "CPRECIO3" double precision NOT NULL DEFAULT 0,
    "CPRECIO4" double precision NOT NULL DEFAULT 0,
    "CPRECIO5" double precision NOT NULL DEFAULT 0,
    "CPRECIO6" double precision NOT NULL DEFAULT 0,
    "CPRECIO7" double precision NOT NULL DEFAULT 0,
    "CPRECIO8" double precision NOT NULL DEFAULT 0,
    "CPRECIO9" double precision NOT NULL DEFAULT 0,
    "CPRECIO10" double precision NOT NULL DEFAULT 0,
    "CBANUNIDADES" integer NOT NULL DEFAULT 0,
    "CBANCARACTERISTICAS" integer NOT NULL DEFAULT 0,
    "CBANMETODOCOSTEO" integer NOT NULL DEFAULT 0,
    "CBANMAXMIN" integer NOT NULL DEFAULT 0,
    "CBANPRECIO" integer NOT NULL DEFAULT 0,
    "CBANIMPUESTO" integer NOT NULL DEFAULT 0,
    "CBANCODIGOBARRA" integer NOT NULL DEFAULT 0,
    "CBANCOMPONENTE" integer NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT '',
    "CERRORCOSTO" integer NOT NULL DEFAULT 0,
    "CFECHAERRORCOSTO" timestamp NOT NULL DEFAULT now(),
    "CPRECIOCALCULADO" double precision NOT NULL DEFAULT 0,
    "CESTADOPRECIO" integer NOT NULL DEFAULT 0,
    "CBANUBICACION" integer NOT NULL DEFAULT 0,
    "CESEXENTO" integer NOT NULL DEFAULT 0,
    "CEXISTENCIANEGATIVA" integer NOT NULL DEFAULT 0,
    "CCOSTOEXT1" double precision NOT NULL DEFAULT 0,
    "CCOSTOEXT2" double precision NOT NULL DEFAULT 0,
    "CCOSTOEXT3" double precision NOT NULL DEFAULT 0,
    "CCOSTOEXT4" double precision NOT NULL DEFAULT 0,
    "CCOSTOEXT5" double precision NOT NULL DEFAULT 0,
    "CFECCOSEX1" timestamp NOT NULL DEFAULT now(),
    "CFECCOSEX2" timestamp NOT NULL DEFAULT now(),
    "CFECCOSEX3" timestamp NOT NULL DEFAULT now(),
    "CFECCOSEX4" timestamp NOT NULL DEFAULT now(),
    "CFECCOSEX5" timestamp NOT NULL DEFAULT now(),
    "CMONCOSEX1" integer NOT NULL DEFAULT 0,
    "CMONCOSEX2" integer NOT NULL DEFAULT 0,
    "CMONCOSEX3" integer NOT NULL DEFAULT 0,
    "CMONCOSEX4" integer NOT NULL DEFAULT 0,
    "CMONCOSEX5" integer NOT NULL DEFAULT 0,
    "CBANCOSEX" integer NOT NULL DEFAULT 0,
    "CESCUOTAI2" integer NOT NULL DEFAULT 0,
    "CESCUOTAI3" integer NOT NULL DEFAULT 0,
    "CIDUNIDADCOMPRA" integer NOT NULL DEFAULT 0,
    "CIDUNIDADVENTA" integer NOT NULL DEFAULT 0,
    "CSUBTIPO" integer NOT NULL DEFAULT 0,
    "CCODALTERN" varchar(30) NOT NULL DEFAULT '',
    "CNOMALTERN" varchar(60) NOT NULL DEFAULT '',
    "CDESCCORTA" varchar(30) NOT NULL DEFAULT '',
    "CIDMONEDA" integer NOT NULL DEFAULT 0,
    "CUSABASCU" integer NOT NULL DEFAULT 0,
    "CTIPOPAQUE" integer NOT NULL DEFAULT 0,
    "CPRECSELEC" integer NOT NULL DEFAULT 0,
    "CDESGLOSAI2" integer NOT NULL DEFAULT 0,
    "CSEGCONTPRODUCTO4" varchar(20) NOT NULL DEFAULT '',
    "CSEGCONTPRODUCTO5" varchar(20) NOT NULL DEFAULT '',
    "CSEGCONTPRODUCTO6" varchar(20) NOT NULL DEFAULT '',
    "CSEGCONTPRODUCTO7" varchar(20) NOT NULL DEFAULT '',
    "CCTAPRED" varchar(150) NOT NULL DEFAULT '',
    "CNODESCOMP" integer NOT NULL DEFAULT 0,
    "CIDUNIXML" integer NOT NULL DEFAULT 0,
    "CNOMODCOMP" integer NOT NULL DEFAULT 0,
    "CCLAVESAT" varchar(8) NOT NULL DEFAULT '',
    "CCANTIDADFISCAL" double precision NOT NULL DEFAULT 0,
    "CUNIDADDIMENSION" integer NOT NULL DEFAULT 0,
    "CALTO" double precision NOT NULL DEFAULT 0,
    "CLARGO" double precision NOT NULL DEFAULT 0,
    "CANCHO" double precision NOT NULL DEFAULT 0
);

-- admAlmacenes
CREATE TABLE IF NOT EXISTS "admAlmacenes" (
    "CIDALMACEN" integer PRIMARY KEY,
    "CCODIGOALMACEN" varchar(30) NOT NULL DEFAULT '',
    "CNOMBREALMACEN" varchar(60) NOT NULL DEFAULT '',
    "CFECHAALTAALMACEN" timestamp NOT NULL DEFAULT now(),
    "CIDVALORCLASIFICACION1" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION2" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION3" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION4" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION5" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION6" integer NOT NULL DEFAULT 0,
    "CSEGCONTALMACEN" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA1" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA2" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA3" varchar(50) NOT NULL DEFAULT '',
    "CFECHAEXTRA" timestamp NOT NULL DEFAULT now(),
    "CIMPORTEEXTRA1" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA2" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA3" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA4" double precision NOT NULL DEFAULT 0,
    "CBANDOMICILIO" integer NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT '',
    "CSCALMAC2" varchar(50) NOT NULL DEFAULT '',
    "CSCALMAC3" varchar(50) NOT NULL DEFAULT '',
    "CSISTORIG" integer NOT NULL DEFAULT 0
);

-- admAgentes
CREATE TABLE IF NOT EXISTS "admAgentes" (
    "CIDAGENTE" integer PRIMARY KEY,
    "CCODIGOAGENTE" varchar(30) NOT NULL DEFAULT '',
    "CNOMBREAGENTE" varchar(60) NOT NULL DEFAULT '',
    "CFECHAALTAAGENTE" timestamp NOT NULL DEFAULT now(),
    "CTIPOAGENTE" integer NOT NULL DEFAULT 0,
    "CCOMISIONVENTAAGENTE" double precision NOT NULL DEFAULT 0,
    "CCOMISIONCOBROAGENTE" double precision NOT NULL DEFAULT 0,
    "CIDCLIENTE" integer NOT NULL DEFAULT 0,
    "CIDPROVEEDOR" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION1" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION2" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION3" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION4" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION5" integer NOT NULL DEFAULT 0,
    "CIDVALORCLASIFICACION6" integer NOT NULL DEFAULT 0,
    "CSEGCONTAGENTE" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA1" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA2" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA3" varchar(50) NOT NULL DEFAULT '',
    "CFECHAEXTRA" timestamp NOT NULL DEFAULT now(),
    "CIMPORTEEXTRA1" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA2" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA3" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA4" double precision NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT '',
    "CSCAGENTE2" varchar(50) NOT NULL DEFAULT '',
    "CSCAGENTE3" varchar(50) NOT NULL DEFAULT ''
);

-- admMonedas
CREATE TABLE IF NOT EXISTS "admMonedas" (
    "CIDMONEDA" integer PRIMARY KEY,
    "CNOMBREMONEDA" varchar(60) NOT NULL DEFAULT '',
    "CSIMBOLOMONEDA" varchar(1) NOT NULL DEFAULT '',
    "CPOSICIONSIMBOLO" integer NOT NULL DEFAULT 0,
    "CPLURAL" varchar(60) NOT NULL DEFAULT '',
    "CSINGULAR" varchar(60) NOT NULL DEFAULT '',
    "CDESCRIPCIONPROTEGIDA" varchar(60) NOT NULL DEFAULT '',
    "CIDBANDERA" integer NOT NULL DEFAULT 0,
    "CDECIMALESMONEDA" integer NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT '',
    "CCLAVESAT" varchar(3) NOT NULL DEFAULT ''
);

-- admDocumentos
CREATE TABLE IF NOT EXISTS "admDocumentos" (
    "CIDDOCUMENTO" integer PRIMARY KEY,
    "CIDDOCUMENTODE" integer NOT NULL DEFAULT 0,
    "CIDCONCEPTODOCUMENTO" integer NOT NULL DEFAULT 0,
    "CSERIEDOCUMENTO" varchar(11) NOT NULL DEFAULT '',
    "CFOLIO" double precision NOT NULL DEFAULT 0,
    "CFECHA" timestamp NOT NULL DEFAULT now(),
    "CIDCLIENTEPROVEEDOR" integer NOT NULL DEFAULT 0,
    "CRAZONSOCIAL" varchar(60) NOT NULL DEFAULT '',
    "CRFC" varchar(20) NOT NULL DEFAULT '',
    "CIDAGENTE" integer NOT NULL DEFAULT 0,
    "CFECHAVENCIMIENTO" timestamp NOT NULL DEFAULT now(),
    "CFECHAPRONTOPAGO" timestamp NOT NULL DEFAULT now(),
    "CFECHAENTREGARECEPCION" timestamp NOT NULL DEFAULT now(),
    "CFECHAULTIMOINTERES" timestamp NOT NULL DEFAULT now(),
    "CIDMONEDA" integer NOT NULL DEFAULT 0,
    "CTIPOCAMBIO" double precision NOT NULL DEFAULT 0,
    "CREFERENCIA" varchar(20) NOT NULL DEFAULT '',
    "COBSERVACIONES" text NULL,
    "CNATURALEZA" integer NOT NULL DEFAULT 0,
    "CIDDOCUMENTOORIGEN" integer NOT NULL DEFAULT 0,
    "CPLANTILLA" integer NOT NULL DEFAULT 0,
    "CUSACLIENTE" integer NOT NULL DEFAULT 1,
    "CUSAPROVEEDOR" integer NOT NULL DEFAULT 0,
    "CAFECTADO" integer NOT NULL DEFAULT 0,
    "CIMPRESO" integer NOT NULL DEFAULT 0,
    "CCANCELADO" integer NOT NULL DEFAULT 0,
    "CDEVUELTO" integer NOT NULL DEFAULT 0,
    "CIDPREPOLIZA" integer NOT NULL DEFAULT 0,
    "CIDPREPOLIZACANCELACION" integer NOT NULL DEFAULT 0,
    "CESTADOCONTABLE" integer NOT NULL DEFAULT 0,
    "CNETO" double precision NOT NULL DEFAULT 0,
    "CIMPUESTO1" double precision NOT NULL DEFAULT 0,
    "CIMPUESTO2" double precision NOT NULL DEFAULT 0,
    "CIMPUESTO3" double precision NOT NULL DEFAULT 0,
    "CRETENCION1" double precision NOT NULL DEFAULT 0,
    "CRETENCION2" double precision NOT NULL DEFAULT 0,
    "CDESCUENTOMOV" double precision NOT NULL DEFAULT 0,
    "CDESCUENTODOC1" double precision NOT NULL DEFAULT 0,
    "CDESCUENTODOC2" double precision NOT NULL DEFAULT 0,
    "CGASTO1" double precision NOT NULL DEFAULT 0,
    "CGASTO2" double precision NOT NULL DEFAULT 0,
    "CGASTO3" double precision NOT NULL DEFAULT 0,
    "CTOTAL" double precision NOT NULL DEFAULT 0,
    "CPENDIENTE" double precision NOT NULL DEFAULT 0,
    "CTOTALUNIDADES" double precision NOT NULL DEFAULT 0,
    "CDESCUENTOPRONTOPAGO" double precision NOT NULL DEFAULT 0,
    "CPORCENTAJEIMPUESTO1" double precision NOT NULL DEFAULT 0,
    "CPORCENTAJEIMPUESTO2" double precision NOT NULL DEFAULT 0,
    "CPORCENTAJEIMPUESTO3" double precision NOT NULL DEFAULT 0,
    "CPORCENTAJERETENCION1" double precision NOT NULL DEFAULT 0,
    "CPORCENTAJERETENCION2" double precision NOT NULL DEFAULT 0,
    "CPORCENTAJEINTERES" double precision NOT NULL DEFAULT 0,
    "CTEXTOEXTRA1" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA2" varchar(50) NOT NULL DEFAULT '',
    "CTEXTOEXTRA3" varchar(50) NOT NULL DEFAULT '',
    "CFECHAEXTRA" timestamp NOT NULL DEFAULT now(),
    "CIMPORTEEXTRA1" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA2" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA3" double precision NOT NULL DEFAULT 0,
    "CIMPORTEEXTRA4" double precision NOT NULL DEFAULT 0,
    "CDESTINATARIO" varchar(60) NOT NULL DEFAULT '',
    "CNUMEROGUIA" varchar(60) NOT NULL DEFAULT '',
    "CMENSAJERIA" varchar(20) NOT NULL DEFAULT '',
    "CCUENTAMENSAJERIA" varchar(120) NOT NULL DEFAULT '',
    "CNUMEROCAJAS" double precision NOT NULL DEFAULT 0,
    "CPESO" double precision NOT NULL DEFAULT 0,
    "CBANOBSERVACIONES" integer NOT NULL DEFAULT 0,
    "CBANDATOSENVIO" integer NOT NULL DEFAULT 0,
    "CBANCONDICIONESCREDITO" integer NOT NULL DEFAULT 0,
    "CBANGASTOS" integer NOT NULL DEFAULT 0,
    "CUNIDADESPENDIENTES" double precision NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT '',
    "CIMPCHEQPAQ" double precision NOT NULL DEFAULT 0,
    "CSISTORIG" integer NOT NULL DEFAULT 0,
    "CIDMONEDCA" integer NOT NULL DEFAULT 0,
    "CTIPOCAMCA" double precision NOT NULL DEFAULT 0,
    "CESCFD" integer NOT NULL DEFAULT 0,
    "CTIENECFD" integer NOT NULL DEFAULT 0,
    "CLUGAREXPE" varchar(380) NOT NULL DEFAULT '',
    "CMETODOPAG" varchar(100) NOT NULL DEFAULT '',
    "CNUMPARCIA" integer NOT NULL DEFAULT 0,
    "CCANTPARCI" integer NOT NULL DEFAULT 0,
    "CCONDIPAGO" varchar(253) NOT NULL DEFAULT '',
    "CNUMCTAPAG" varchar(100) NOT NULL DEFAULT '',
    "CGUIDDOCUMENTO" varchar(40) NOT NULL DEFAULT '',
    "CUSUARIO" varchar(15) NOT NULL DEFAULT '',
    "CIDPROYECTO" integer NOT NULL DEFAULT 0,
    "CIDCUENTA" integer NOT NULL DEFAULT 0,
    "CTRANSACTIONID" varchar(26) NOT NULL DEFAULT '',
    "CIDCOPIADE" integer NOT NULL DEFAULT 0,
    "CVERESQUE" varchar(6) NOT NULL DEFAULT '',
    "CDATOSADICIONALES" text NULL,
    "CIDAPERTURA" integer NOT NULL DEFAULT 0
);

-- admMovimientos
CREATE TABLE IF NOT EXISTS "admMovimientos" (
    "CIDMOVIMIENTO" integer PRIMARY KEY,
    "CIDDOCUMENTO" integer NOT NULL DEFAULT 0,
    "CIDDOCUMENTODE" integer NOT NULL DEFAULT 0,
    "CNUMEROMOVIMIENTO" integer NOT NULL DEFAULT 0,
    "CIDPRODUCTO" integer NOT NULL DEFAULT 0,
    "CIDALMACEN" integer NOT NULL DEFAULT 0,
    "CUNIDADES" double precision NOT NULL DEFAULT 0,
    "CPRECIO" double precision NOT NULL DEFAULT 0,
    "CTOTAL" double precision NOT NULL DEFAULT 0,
    "CDESCUENTOMOV" double precision NOT NULL DEFAULT 0,
    "COBSERVACIONES" text NULL,
    "CIDPROYECTO" integer NOT NULL DEFAULT 0,
    "CIDCUENTA" integer NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT ''
);

-- admConceptos
CREATE TABLE IF NOT EXISTS "admConceptos" (
    "CIDCONCEPTODOCUMENTO" integer PRIMARY KEY,
    "CIDDOCUMENTODE" integer NOT NULL DEFAULT 0,
    "CNOMBRECONCEPTO" varchar(100) NOT NULL DEFAULT '',
    "CNATURALEZACONCEPTO" integer NOT NULL DEFAULT 0,
    "CNOFOLIO" double precision NOT NULL DEFAULT 0,
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT ''
);

-- admDocumentosModelo
CREATE TABLE IF NOT EXISTS "admDocumentosModelo" (
    "CIDDOCUMENTODE" integer PRIMARY KEY,
    "CNOMBREDOCUMENTO" varchar(100) NOT NULL DEFAULT '',
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT ''
);

-- admUnidadesMedidaPeso
CREATE TABLE IF NOT EXISTS "admUnidadesMedidaPeso" (
    "CIDUNIDAD" integer PRIMARY KEY,
    "CCODIGOUNIDAD" varchar(10) NOT NULL DEFAULT '',
    "CNOMBREUNIDAD" varchar(50) NOT NULL DEFAULT '',
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT ''
);

-- admDomicilios
CREATE TABLE IF NOT EXISTS "admDomicilios" (
    "CIDDOMICILIO" integer PRIMARY KEY,
    "CIDCATALOGO" integer NOT NULL DEFAULT 0,
    "CTIPOCATALOGO" integer NOT NULL DEFAULT 0,
    "CTIPODIRECCION" integer NOT NULL DEFAULT 0,
    "CCALLE" varchar(100) NOT NULL DEFAULT '',
    "CNUMEROEXTERIOR" varchar(10) NOT NULL DEFAULT '',
    "CNUMEROINTERIOR" varchar(10) NOT NULL DEFAULT '',
    "CCOLONIA" varchar(100) NOT NULL DEFAULT '',
    "CCODIGOPOSTAL" varchar(10) NOT NULL DEFAULT '',
    "CCIUDAD" varchar(100) NOT NULL DEFAULT '',
    "CESTADO" varchar(50) NOT NULL DEFAULT '',
    "CPAIS" varchar(50) NOT NULL DEFAULT '',
    "CTIMESTAMP" varchar(23) NOT NULL DEFAULT ''
);

-- =============================================================================
-- 5. Ajustes finales de consistencia
-- =============================================================================

-- fleet_vehiculos: corregir tipo de kilometraje_actual para que coincida con EF (int)
ALTER TABLE fleet_vehiculos ALTER COLUMN kilometraje_actual TYPE integer USING kilometraje_actual::integer;

-- finance_gastos_viaticos: cambiar fecha a timestamp para coincidir con el modelo EF
ALTER TABLE finance_gastos_viaticos ALTER COLUMN fecha TYPE timestamp USING fecha::timestamp;

-- evaluaciones: eliminar columnas heredadas incorrectamente de ejecuciones_orden
-- y agregar las columnas que el modelo EF Evaluacion espera.
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS tipo_ejecucion CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS tecnico_id CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS hr_inicio CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS hr_fin CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS comentarios CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS vehiculo_id CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS km_inicial CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS km_final CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS herramientas CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS codigo_sesion CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS contrasena_sesion CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS evaluacion_actualizada CASCADE;
ALTER TABLE evaluaciones DROP COLUMN IF EXISTS nueva_evaluacion CASCADE;

ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS ejecucion_id integer NULL;
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS cliente_id integer NULL;
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS evaluador_id integer NOT NULL DEFAULT 0;
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS objetivo varchar(200) NULL;
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS comentarios_generales text NULL;
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS score_calidad_total integer NULL CHECK (score_calidad_total IS NULL OR (score_calidad_total >= 0 AND score_calidad_total <= 100));
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS requiere_seguimiento boolean NOT NULL DEFAULT false;
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS seguimiento_notas text NULL;
ALTER TABLE evaluaciones ADD COLUMN IF NOT EXISTS creado_en timestamp NOT NULL DEFAULT now();

-- files_documentos: agregar checksum_sha256 que falta en el modelo EF
ALTER TABLE files_documentos ADD COLUMN IF NOT EXISTS checksum_sha256 varchar(64) NULL;
