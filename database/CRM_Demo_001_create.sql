-- =============================================================================
-- CRM_Demo - Esquema para Supabase (PostgreSQL)
-- -----------------------------------------------------------------------------
-- Proposito: base de datos DEMO limpia para el MVP de portafolio de FullStack_CABS.
-- Convertido desde el esquema original SQL Server (BD_CRM_completp.sql).
--
-- Cambios aplicados respecto al original:
--   * Eliminadas TODAS las rutas hardcodeadas (.mdf/.ldf/.ndf) y el CREATE/ALTER DATABASE.
--   * Esquema unico `public` (el original usaba auth/catalog/legacy/dbo/ops/fleet/files/finance).
--   * Identificadores en snake_case (minusculas) para evitar comillas en PostgreSQL.
--   * Tipos SQL Server -> PostgreSQL: varchar(N)/nvarchar -> varchar(N); nvarchar(max) -> text;
--     datetime/datetime2 -> timestamp; date -> date; time -> time; bit -> boolean;
--     decimal(N,S)/money -> numeric(N,S); float -> double precision; uniqueidentifier -> uuid;
--     int/bigint/smallint -> int/bigint/smallint; JSON (isjson) -> jsonb.
--   * IDENTITY(1,1) -> GENERATED ALWAYS AS IDENTITY.
--   * Columnas computadas (AS) -> GENERATED ALWAYS AS (...) STORED.
--   * DEFAULT getdate()/sysutcdatetime() -> now().
--
-- NOTA: el esquema original tenia tablas DUPLICADAS (contexto "nuevo" y contexto
-- "legacy"/dbo). Se conservan ambas versiones con nombres distintos porque el
-- backend tiene contextos separados (WriteContext/ReadOnlyContext/LegacyCompacContext).
-- Si mas adelante se consolida a un solo contexto, se pueden fusionar.
--
-- Como usar en Supabase:
--   1. Abrir el SQL Editor del proyecto Supabase.
--   2. Pegar y ejecutar este archivo (CRM_Demo_001_create.sql).
--   3. (Opcional) ejecutar CRM_Demo_002_seed.sql para datos demo.
--   4. La app se conecta via la clave service_role (ver connection keys que se
--      enviaran aparte). Para el MVP se puede dejar RLS desactivado; ver final.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Tablas de referencia legacy (sin FKs, se crean primero)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS legacy_clientes_ref (
    legacy_client_id integer PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS legacy_productos_ref (
    legacy_product_id integer PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS legacy_intakes_ref (
    legacy_intake_id integer PRIMARY KEY
);

-- -----------------------------------------------------------------------------
-- Usuarios (auth "nuevo" y auth "legacy/dbo")
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS usuarios (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    creado_en          timestamp NOT NULL DEFAULT now(),
    actualizado_en     timestamp NULL,
    correo             varchar(150) NOT NULL UNIQUE,
    password_hash      varchar(255) NOT NULL,
    nombre             varchar(100) NOT NULL,
    apellido           varchar(100) NOT NULL,
    telefono           varchar(20)  NULL,
    rol                varchar(30)  NOT NULL CHECK (rol IN ('SUPERADMIN','ADMINISTRACION','SOPORTE','RECEPCION')),
    activo             boolean       NOT NULL DEFAULT true,
    licencia_conducir varchar(50)  NULL,
    transmision_habilitada varchar(50) NULL
);

CREATE TABLE IF NOT EXISTS auth_usuarios (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    creado_en          timestamp NOT NULL DEFAULT now(),
    actualizado_en     timestamp NULL,
    correo             varchar(150) NOT NULL UNIQUE,
    password_hash      varchar(255) NOT NULL,
    nombre             varchar(100) NOT NULL,
    apellido           varchar(100) NOT NULL,
    telefono           bigint        NOT NULL,
    rol                varchar(30)  NOT NULL CHECK (rol IN ('ADMINISTRACION','SOPORTE','RECEPCION')),
    activo             boolean       NOT NULL DEFAULT true,
    transmision_habilitada varchar(50) NULL,
    id_agente_legacy  integer NULL,
    codigo_agente_legacy varchar(30) NULL,
    nombre_agente_legacy varchar(60) NULL,
    fecha_enlace_agente  timestamp NULL
);

-- -----------------------------------------------------------------------------
-- Catalogos / clientes / productos (nuevo y legacy/dbo)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS clientes (
    id               integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    legacy_client_id integer NULL UNIQUE,
    nombre_comercial varchar(200) NULL,
    nombre           varchar(100) NULL,
    apellido         varchar(100) NULL,
    rfc              varchar(13)  NULL,
    estado_fiscal    varchar(50)  NULL,
    telefono         varchar(20)  NULL,
    email            varchar(150) NULL,
    activo          boolean       NOT NULL DEFAULT true,
    created_at       timestamp NOT NULL DEFAULT now(),
    direccion_json   jsonb         NULL
);

CREATE TABLE IF NOT EXISTS catalog_clientes (
    id               integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    legacy_client_id integer NULL UNIQUE,
    nombre_comercial varchar(200) NULL,
    nombre           varchar(100) NULL,
    apellido         varchar(100) NULL,
    rfc              varchar(13)  NULL,
    estado_fiscal    varchar(50)  NULL,
    telefono         varchar(20)  NULL,
    email            varchar(150) NULL,
    activo          boolean       NOT NULL DEFAULT true,
    created_at       timestamp NOT NULL DEFAULT now(),
    direccion_json   jsonb         NULL,
    CONSTRAINT fk_catalog_clientes_legacy FOREIGN KEY (legacy_client_id)
        REFERENCES legacy_clientes_ref (legacy_client_id)
);

CREATE TABLE IF NOT EXISTS productos_servicio_ref (
    id              integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre          varchar(200) NOT NULL,
    tipo            varchar(50)  NOT NULL,
    unidad          varchar(20)  NOT NULL,
    precio_lista    numeric(12,2) NOT NULL,
    legacy_product_id integer NULL,
    CONSTRAINT fk_prod_ref_legacy FOREIGN KEY (legacy_product_id)
        REFERENCES legacy_productos_ref (legacy_product_id)
);

CREATE TABLE IF NOT EXISTS catalog_productos_servicio_ref (
    id              integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nombre          varchar(200) NOT NULL,
    tipo            varchar(50)  NOT NULL,
    unidad          varchar(20)  NOT NULL,
    precio_lista    numeric(12,2) NOT NULL,
    legacy_product_id integer NULL,
    CONSTRAINT fk_cat_prod_ref_legacy FOREIGN KEY (legacy_product_id)
        REFERENCES legacy_productos_ref (legacy_product_id)
);

-- -----------------------------------------------------------------------------
-- Flota / vehiculos (nuevo y legacy/dbo)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS fleet_vehiculos (
    id               integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tipo_vehiculo   varchar(50)  NULL,
    transmision     varchar(20)  NULL,
    es_de_empresa   boolean       NOT NULL DEFAULT true,
    placas          varchar(20)  NULL UNIQUE,
    activo          boolean       NOT NULL DEFAULT true,
    observaciones   text          NULL,
    nombre_vehiculo varchar(100) NOT NULL DEFAULT '',
    kilometraje_actual numeric(10,2) NULL,
    ultimo_uso      timestamp NULL
);

CREATE TABLE IF NOT EXISTS vehiculos (
    id               integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tipo_vehiculo   varchar(50)  NULL,
    transmision     varchar(20)  NULL,
    es_de_empresa   boolean       NOT NULL DEFAULT true,
    placas          varchar(20)  NULL UNIQUE,
    activo          boolean       NOT NULL DEFAULT true,
    observaciones   text          NULL,
    nombre_vehiculo varchar(100) NOT NULL DEFAULT '',
    kilometraje     integer       NULL
);

CREATE TABLE IF NOT EXISTS fleet_uso_vehiculos (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    vehiculo_id         integer NOT NULL,
    usuario_id          integer NOT NULL,
    fecha               date    NOT NULL,
    hora_salida         time    NOT NULL,
    hora_regreso        time    NULL,
    motivo_uso         varchar(500) NOT NULL,
    kilometraje_inicial integer NOT NULL,
    kilometraje_final   integer NULL,
    observaciones       text          NULL,
    firma_base64       text          NULL,
    estado             varchar(20)  NOT NULL DEFAULT 'EN_USO'
        CHECK (estado IN ('CANCELADO','COMPLETADO','EN_USO')),
    fecha_creacion     timestamp NOT NULL DEFAULT now(),
    fecha_actualizacion timestamp NULL,
    CONSTRAINT fk_fleet_uso_usuario FOREIGN KEY (usuario_id)
        REFERENCES auth_usuarios (id),
    CONSTRAINT fk_fleet_uso_vehiculo FOREIGN KEY (vehiculo_id)
        REFERENCES fleet_vehiculos (id),
    CONSTRAINT ck_fleet_uso_km CHECK (kilometraje_final IS NULL OR kilometraje_final >= kilometraje_inicial)
);

-- -----------------------------------------------------------------------------
-- Ordenes de trabajo (nuevo y legacy/dbo) + ejecuciones
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ops_ordenes_trabajo (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cliente_id          integer NOT NULL,
    creado_por_user_id integer NOT NULL,
    asignada_a_user_id integer NULL,
    notas              text          NULL,
    cita_programada_inicio  timestamp NULL,
    cita_programada_fin     timestamp NULL,
    modalidad          varchar(50)  NULL,
    tipo_orden         varchar(50)  NULL,
    prioridad          integer       NULL,
    estado             varchar(20)  NOT NULL
        CHECK (estado IN ('CERRADA','FACTURADA','POR_FACTURAR','COMPLETADA','EN_CURSO','ASIGNADA','CAPTURADA')),
    ubicacion_text     text          NULL,
    requiere_factura   boolean       NOT NULL DEFAULT false,
    estado_facturado   varchar(50)  NULL,
    factura_folio      varchar(50)  NULL,
    creado_en          timestamp NOT NULL DEFAULT now(),
    actualizado_en     timestamp NULL,
    costo_real         numeric(12,2) NULL,
    costo_estimado     numeric(12,2) NULL,
    cliente_telefono   bigint        NULL,
    CONSTRAINT fk_orden_cliente FOREIGN KEY (cliente_id)
        REFERENCES catalog_clientes (id),
    CONSTRAINT fk_orden_creado_por FOREIGN KEY (creado_por_user_id)
        REFERENCES auth_usuarios (id),
    CONSTRAINT fk_orden_asignada_a FOREIGN KEY (asignada_a_user_id)
        REFERENCES auth_usuarios (id)
);

CREATE TABLE IF NOT EXISTS ordenes_trabajo (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nuevo_cliente       boolean       NULL,
    nombre_cliente      varchar(120) NULL,
    cliente_id          integer       NULL,
    creado_por_user_id integer       NOT NULL,
    asignada_a_user_id integer       NULL,
    notas              text          NULL,
    cita_programada_inicio  timestamp NULL,
    cita_programada_fin     timestamp NULL,
    modalidad          varchar(50)  NULL,
    tipo_orden         varchar(50)  NULL,
    prioridad          integer       NULL,
    estado             varchar(20)  NOT NULL
        CHECK (estado IN ('CERRADA','FACTURADA','POR_FACTURAR','COMPLETADA','EN_CURSO','ASIGNADA','CAPTURADA')),
    ubicacion_text     text          NULL,
    requiere_factura   boolean       NOT NULL DEFAULT false,
    estado_facturado   varchar(50)  NULL,
    factura_folio      varchar(50)  NULL,
    creado_en          timestamp NOT NULL DEFAULT now(),
    actualizado_en     timestamp NULL,
    costo_real         numeric(12,2) NULL,
    costo_estimado     numeric(12,2) NULL,
    cliente_telefono   bigint        NULL,
    CONSTRAINT fk_ot_cliente FOREIGN KEY (cliente_id)
        REFERENCES clientes (id),
    CONSTRAINT fk_ot_creado_por FOREIGN KEY (creado_por_user_id)
        REFERENCES auth_usuarios (id),
    CONSTRAINT fk_ot_asignada_a FOREIGN KEY (asignada_a_user_id)
        REFERENCES auth_usuarios (id)
);

CREATE TABLE IF NOT EXISTS ops_ejecuciones_orden (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    orden_id       integer NOT NULL,
    tipo_ejecucion varchar(20) NOT NULL
        CHECK (tipo_ejecucion IN ('CAMPO','REMOTO')),
    tecnico_id     integer NOT NULL,
    hr_inicio      timestamp NULL,
    hr_fin         timestamp NULL,
    comentarios    text      NULL,
    vehiculo_id   integer   NULL,
    km_inicial     integer   NULL,
    km_final       integer   NULL,
    herramientas   varchar(100) NULL,
    codigo_sesion  varchar(100) NULL,
    contrasena_sesion varchar(100) NULL,
    CONSTRAINT fk_ejecucion_orden FOREIGN KEY (orden_id)
        REFERENCES ops_ordenes_trabajo (id) ON DELETE CASCADE,
    CONSTRAINT fk_ejecucion_tecnico FOREIGN KEY (tecnico_id)
        REFERENCES auth_usuarios (id),
    CONSTRAINT fk_ejecucion_vehiculo FOREIGN KEY (vehiculo_id)
        REFERENCES fleet_vehiculos (id)
);

CREATE TABLE IF NOT EXISTS ejecuciones_orden (
    id             integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    orden_id       integer NOT NULL,
    tipo_ejecucion varchar(20) NOT NULL
        CHECK (tipo_ejecucion IN ('CAMPO','REMOTO')),
    tecnico_id     integer NOT NULL,
    hr_inicio      timestamp NULL,
    hr_fin         timestamp NULL,
    comentarios    text      NULL,
    vehiculo_id   integer   NULL,
    km_inicial     integer   NULL,
    km_final       integer   NULL,
    herramientas   varchar(100) NULL,
    codigo_sesion  varchar(100) NULL,
    contrasena_sesion varchar(100) NULL,
    evaluacion_actualizada varchar(255) NULL,
    nueva_evaluacion varchar(255) NULL,
    CONSTRAINT fk_ej_orden FOREIGN KEY (orden_id)
        REFERENCES ordenes_trabajo (id) ON DELETE CASCADE,
    CONSTRAINT fk_ej_tecnico FOREIGN KEY (tecnico_id)
        REFERENCES auth_usuarios (id),
    CONSTRAINT fk_ej_vehiculo FOREIGN KEY (vehiculo_id)
        REFERENCES vehiculos (id)
);

-- -----------------------------------------------------------------------------
-- Evaluaciones (nuevo y legacy/dbo)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS evaluaciones (
    id                   integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    orden_id             integer NOT NULL,
    ejecucion_id         integer NULL,
    cliente_id           integer NULL,
    evaluador_id         integer NOT NULL,
    objetivo             varchar(200) NULL,
    comentarios_generales text          NULL,
    score_calidad_total  integer       NULL CHECK (score_calidad_total >= 0 AND score_calidad_total <= 100),
    requiere_seguimiento boolean       NOT NULL DEFAULT false,
    seguimiento_notas    text          NULL,
    evaluacion_actualizada varchar(255) NULL,
    nueva_evaluacion     varchar(255) NULL,
    creado_en            timestamp NOT NULL DEFAULT now(),
    CONSTRAINT fk_eval_orden FOREIGN KEY (orden_id)
        REFERENCES ops_ordenes_trabajo (id) ON DELETE CASCADE,
    CONSTRAINT fk_eval_usuario FOREIGN KEY (evaluador_id)
        REFERENCES auth_usuarios (id)
);

CREATE TABLE IF NOT EXISTS evaluacion_detalles (
    id            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    evaluacion_id integer NOT NULL,
    fase          varchar(10) NOT NULL CHECK (fase IN ('DESPUES','ANTES')),
    descripcion    text      NULL,
    sugerencias   text      NULL,
    score_fase    integer   NULL CHECK (score_fase >= 0 AND score_fase <= 100),
    evidencia_nota text      NULL,
    creado_en     timestamp NOT NULL DEFAULT now(),
    lugar         varchar(100) NOT NULL,
    CONSTRAINT fk_eval_detalle_eval FOREIGN KEY (evaluacion_id)
        REFERENCES evaluaciones (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS evaluacion_fotos (
    id            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    detalle_id    integer NOT NULL,
    documento_id integer NOT NULL,
    tipo          varchar(20) NULL,
    descripcion   varchar(500) NULL,
    creado_en     timestamp NOT NULL DEFAULT now(),
    CONSTRAINT fk_eval_fotos_detalle FOREIGN KEY (detalle_id)
        REFERENCES evaluacion_detalles (id) ON DELETE CASCADE,
    CONSTRAINT fk_eval_fotos_documento FOREIGN KEY (documento_id)
        REFERENCES files_documentos (id) ON DELETE CASCADE
);

-- -----------------------------------------------------------------------------
-- Reparaciones (nuevo y legacy/dbo)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS reparaciones (
    id                       integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    orden_id                 integer NOT NULL,
    tecnico_id               integer NOT NULL,
    dispositivo_tipo         varchar(80) NOT NULL,
    marca                    varchar(80) NULL,
    modelo                   varchar(120) NULL,
    accesorios_recibidos    varchar(500) NULL,
    descripcion_falla        text NOT NULL,
    diagnostico             text NULL,
    solucion_aplicada       text NULL,
    resultado                varchar(20) NOT NULL
        CHECK (resultado IN ('DEVUELTO_SIN_REPARAR','COTIZAR','IRREPARABLE','REPARADO')),
    causa_irreparable       text NULL,
    respaldo_datos_autorizado boolean NOT NULL DEFAULT false,
    costo_mano_obra         numeric(12,2) NULL CHECK (costo_mano_obra IS NULL OR costo_mano_obra >= 0),
    costo_refacciones_compra  numeric(12,2) NULL CHECK (costo_refacciones_compra IS NULL OR costo_refacciones_compra >= 0),
    costo_refacciones_publico numeric(12,2) NULL CHECK (costo_refacciones_publico IS NULL OR costo_refacciones_publico >= 0),
    costo_total_compra        numeric(12,2) GENERATED ALWAYS AS (coalesce(costo_mano_obra,0) + coalesce(costo_refacciones_compra,0)) STORED,
    costo_total_publico       numeric(12,2) GENERATED ALWAYS AS (coalesce(costo_mano_obra,0) + coalesce(costo_refacciones_publico,0)) STORED,
    margen_estimado         numeric(12,2) GENERATED ALWAYS AS (coalesce(costo_refacciones_publico,0) - coalesce(costo_refacciones_compra,0)) STORED,
    garantia_dias            integer NULL CHECK (garantia_dias IS NULL OR garantia_dias >= 0),
    fecha_llegada            timestamp NOT NULL DEFAULT now(),
    empezado_en            timestamp NULL,
    entregado_en            timestamp NULL,
    tipo_entrega             varchar(20) NOT NULL CHECK (tipo_entrega IN ('RECOGE_CLIENTE','DOMICILIO')),
    ubicacion_almacenamiento varchar(200) NULL,
    notas                    text NULL,
    CONSTRAINT fk_rep_orden FOREIGN KEY (orden_id)
        REFERENCES ops_ordenes_trabajo (id) ON DELETE CASCADE,
    CONSTRAINT fk_rep_tecnico FOREIGN KEY (tecnico_id)
        REFERENCES auth_usuarios (id)
);

CREATE TABLE IF NOT EXISTS reparacion_componentes (
    id                     integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reparacion_id          integer NOT NULL,
    componente             varchar(160) NOT NULL,
    cantidad               integer NOT NULL DEFAULT 1 CHECK (cantidad > 0),
    proveedor              varchar(160) NULL,
    garantia_meses        integer NULL CHECK (garantia_meses IS NULL OR garantia_meses >= 0),
    costo_unitario_compra  numeric(12,2) NULL CHECK (costo_unitario_compra IS NULL OR costo_unitario_compra >= 0),
    costo_unitario_publico numeric(12,2) NULL CHECK (costo_unitario_publico IS NULL OR costo_unitario_publico >= 0),
    subtotal_compra        numeric(12,2) GENERATED ALWAYS AS (coalesce(cantidad,0) * coalesce(costo_unitario_compra,0)) STORED,
    subtotal_publico        numeric(12,2) GENERATED ALWAYS AS (coalesce(cantidad,0) * coalesce(costo_unitario_publico,0)) STORED,
    notas                  varchar(500) NULL,
    CONSTRAINT fk_rep_comp_rep FOREIGN KEY (reparacion_id)
        REFERENCES reparaciones (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS reparacion_fotos (
    id            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    reparacion_id integer NOT NULL,
    documento_id integer NOT NULL,
    etapa         varchar(20) NULL,
    descripcion   varchar(500) NULL,
    creado_en     timestamp NOT NULL DEFAULT now(),
    CONSTRAINT fk_rep_fotos_documento FOREIGN KEY (documento_id)
        REFERENCES files_documentos (id) ON DELETE CASCADE,
    CONSTRAINT fk_rep_fotos_rep FOREIGN KEY (reparacion_id)
        REFERENCES reparaciones (id) ON DELETE CASCADE
);

-- -----------------------------------------------------------------------------
-- Gastos / viaticos (nuevo y legacy/dbo) + finance
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS finance_gastos_viaticos (
    id              integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    orden_id        integer NOT NULL,
    tiene_factura   boolean NOT NULL DEFAULT false,
    descripcion     text      NULL,
    proveedor_nombre varchar(200) NULL,
    fecha           date    NOT NULL,
    km_recorridos   integer NULL CHECK (km_recorridos IS NULL OR km_recorridos >= 0),
    gastos          varchar(200) NOT NULL,
    monto_total     numeric(12,2) NOT NULL,
    lugar_destino   varchar(200) NULL,
    CONSTRAINT fk_gastos_viaticos_orden FOREIGN KEY (orden_id)
        REFERENCES ops_ordenes_trabajo (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS gastos_viaticos (
    id              integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    orden_id        integer NOT NULL,
    tiene_factura   boolean NOT NULL DEFAULT false,
    descripcion     text      NULL,
    proveedor_nombre varchar(200) NULL,
    fecha           date    NOT NULL,
    km_recorridos   integer NULL CHECK (km_recorridos IS NULL OR km_recorridos >= 0),
    gastos          varchar(200) NOT NULL,
    monto_total     numeric(12,2) NOT NULL,
    lugar_destino   varchar(200) NULL,
    CONSTRAINT fk_viatico_orden FOREIGN KEY (orden_id)
        REFERENCES ordenes_trabajo (id) ON DELETE CASCADE
);

-- -----------------------------------------------------------------------------
-- Archivos / documentos (nuevo y legacy/dbo)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS files_documentos (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    creado_por_usuario_id integer NOT NULL,
    creado_en          timestamp NOT NULL DEFAULT now(),
    entidad_tipo       varchar(50) NOT NULL,
    entidad_id          integer NOT NULL,
    metadatos_json     jsonb        NULL,
    tamano_bytes       bigint       NULL,
    mimetype           varchar(100) NULL,
    ruta_almacenamiento varchar(500) NOT NULL,
    nombre_archivo     varchar(255) NOT NULL,
    checksum_sha256    varchar(64) NOT NULL DEFAULT '',
    nombre_original    varchar(255) NOT NULL DEFAULT '',
    activo             boolean       NOT NULL DEFAULT true,
    actualizado_en     timestamp NULL,
    CONSTRAINT uq_documento_entidad UNIQUE (entidad_tipo, entidad_id, nombre_archivo),
    CONSTRAINT fk_documentos_usuario FOREIGN KEY (creado_por_usuario_id)
        REFERENCES auth_usuarios (id)
);

CREATE TABLE IF NOT EXISTS documentos (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    creado_por_usuario_id integer NOT NULL,
    creado_en          timestamp NOT NULL DEFAULT now(),
    entidad_tipo       varchar(50) NOT NULL,
    entidad_id          integer NOT NULL,
    metadatos_json     jsonb        NULL,
    tamano_bytes       bigint       NULL,
    mimetype           varchar(100) NULL,
    ruta_almacenamiento varchar(500) NOT NULL,
    nombre_archivo     varchar(255) NOT NULL,
    CONSTRAINT uq_doc_entidad UNIQUE (entidad_tipo, entidad_id, nombre_archivo),
    CONSTRAINT fk_doc_usuario FOREIGN KEY (creado_por_usuario_id)
        REFERENCES usuarios (id) ON UPDATE CASCADE
);

-- -----------------------------------------------------------------------------
-- Calendario / notificaciones / horarios
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS calendario_eventos (
    id                  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    creado_por_id       integer NOT NULL,
    titulo              varchar(255) NULL,
    descripcion         text      NULL,
    fecha_inicio        date    NOT NULL,
    fecha_fin           date    NOT NULL,
    hora_inicio         time    NOT NULL,
    hora_fin            time    NOT NULL,
    tipo_evento         varchar(50) NULL,
    es_compartido       boolean NOT NULL,
    color               varchar(50) NULL,
    fecha_creacion      timestamp NOT NULL DEFAULT now(),
    ultima_modificacion timestamp NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS calendarios_evento_permisos (
    id               integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    evento_id        integer NOT NULL,
    usuario_id       integer NOT NULL,
    tipo_permiso     varchar(50) NULL,
    fecha_asignacion timestamp NOT NULL DEFAULT now(),
    CONSTRAINT fk_evento_permisos_evento FOREIGN KEY (evento_id)
        REFERENCES calendario_eventos (id) ON DELETE CASCADE,
    CONSTRAINT fk_evento_permisos_usuario FOREIGN KEY (usuario_id)
        REFERENCES auth_usuarios (id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS notificaciones (
    id               integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usuario_id        integer NOT NULL,
    tipo             varchar(50)  NOT NULL,
    titulo           varchar(200) NOT NULL,
    mensaje          varchar(1000) NOT NULL,
    datos            jsonb         NULL,
    leida            boolean       NOT NULL DEFAULT false,
    fecha_creacion   timestamp NOT NULL DEFAULT now(),
    fecha_lectura    timestamp NULL,
    prioridad        varchar(20)  NOT NULL DEFAULT 'MEDIA',
    accion           varchar(500) NULL,
    CONSTRAINT fk_notificaciones_usuario FOREIGN KEY (usuario_id)
        REFERENCES auth_usuarios (id)
);

CREATE TABLE IF NOT EXISTS usuario_horarios (
    id            integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    usuario_id    integer NOT NULL,
    nombre        varchar(100) NOT NULL,
    dia_inicio    varchar(20) NOT NULL,
    dia_fin       varchar(20) NOT NULL,
    hora_inicio   time    NOT NULL,
    hora_fin      time    NOT NULL,
    is_compartido boolean NOT NULL DEFAULT false,
    creado_en     timestamp NOT NULL DEFAULT now(),
    actualizado_en timestamp NULL,
    CONSTRAINT fk_usuario_horarios_auth FOREIGN KEY (usuario_id)
        REFERENCES auth_usuarios (id) ON UPDATE CASCADE ON DELETE CASCADE
);

-- -----------------------------------------------------------------------------
-- Recuperacion de password (singular y plural en el original)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS recuperacion_password_token (
    id         integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email      varchar(256) NULL,
    token      varchar(6) NULL,
    expiracion timestamp NULL,
    usado      boolean NULL DEFAULT false
);

CREATE TABLE IF NOT EXISTS recuperacion_password_tokens (
    id         integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email      varchar(255) NOT NULL,
    token      varchar(10) NOT NULL,
    expiracion timestamp NOT NULL,
    usado      boolean NOT NULL DEFAULT false,
    created_at timestamp NOT NULL DEFAULT now()
);

-- -----------------------------------------------------------------------------
-- Cotizaciones legacy (estilo Adminpaq) - solo si se usa el contexto legacy
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS sales_cotizaciones (
    cid_documento        integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cid_documento_de    integer NOT NULL,
    cid_concepto_documento integer NOT NULL,
    cid_cliente_proveedor integer NOT NULL,
    cid_agente          integer NOT NULL,
    cid_documento_origen integer NULL,
    cserie_documento   varchar(11) NULL,
    cfolio              double precision NOT NULL DEFAULT 0,
    cfeha               timestamp NOT NULL DEFAULT now(),
    cfeha_vencimiento  timestamp NULL,
    cfeha_entrega_recepcion timestamp NULL,
    crazon_social      varchar(60) NULL,
    crfc                varchar(20) NULL,
    creferencia        varchar(20) NULL,
    cobservaciones     text NULL,
    cnaturaleza        integer NOT NULL DEFAULT 0,
    cusacliente         integer NOT NULL DEFAULT 1,
    cafectado          integer NOT NULL DEFAULT 0,
    cimpuesto1        integer NOT NULL DEFAULT 0,
    ccancelado         integer NOT NULL DEFAULT 0,
    cneto              double precision NOT NULL DEFAULT 0,
    cdescuentomov     double precision NOT NULL DEFAULT 0,
    ctotal             double precision NOT NULL DEFAULT 0,
    cpendiente         double precision NOT NULL DEFAULT 0,
    ctotalunidades     double precision NOT NULL DEFAULT 0
);

-- -----------------------------------------------------------------------------
-- Tablas de referencia legacy (duplicado del esquema legacy.*)
-- No tienen FKs hacia ellas, pero se incluyen para ser fieles a las
-- 36 tablas del esquema original y no romper mapeos de EF Core.
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS clientes_ref (
    legacy_client_id integer PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS intakes_ref (
    legacy_intake_id integer PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS productos_ref (
    legacy_product_id integer PRIMARY KEY
);

-- =============================================================================
-- NOTAS SUPABASE / ROW LEVEL SECURITY (RLS)
-- -----------------------------------------------------------------------------
-- Para el MVP la app se conecta con la clave `service_role` (anon/autenticado a
-- nivel de API), por lo que RLS puede quedar DESACTIVADO. Si mas adelante quieres
-- proteger por fila (multi-tenant / por usuario), activa RLS asi (ejemplo):
--
--   ALTER TABLE usuarios ENABLE ROW LEVEL SECURITY;
--   CREATE POLICY "usuarios visibles para el propio usuario"
--     ON usuarios FOR SELECT
--     USING (id = current_setting('app.user_id')::integer);
--
-- Recuerda: al activar RLS necesitas policies para cada operacion (SELECT/INSERT/UPDATE/DELETE).
-- =============================================================================
