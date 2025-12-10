USE [master]
GO

/****** Object:  Database [CABS_Pruebas]    Script Date: 2/12/2025 11:35:49 ******/
CREATE DATABASE [CABS_Pruebas]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'CABS_Pruebas', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\CABS_Pruebas.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'CABS_Pruebas_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\CABS_Pruebas_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CABS_Pruebas].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [CABS_Pruebas] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET ARITHABORT OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [CABS_Pruebas] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [CABS_Pruebas] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET  ENABLE_BROKER 
GO

ALTER DATABASE [CABS_Pruebas] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [CABS_Pruebas] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET RECOVERY FULL 
GO

ALTER DATABASE [CABS_Pruebas] SET  MULTI_USER 
GO

ALTER DATABASE [CABS_Pruebas] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [CABS_Pruebas] SET DB_CHAINING OFF 
GO

ALTER DATABASE [CABS_Pruebas] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [CABS_Pruebas] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [CABS_Pruebas] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [CABS_Pruebas] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO

ALTER DATABASE [CABS_Pruebas] SET QUERY_STORE = ON
GO

ALTER DATABASE [CABS_Pruebas] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO

ALTER DATABASE [CABS_Pruebas] SET  READ_WRITE 
GO



USE [CABS_Pruebas]
GO

/****** Object:  Table [auth].[usuarios]    Script Date: 3/12/2025 10:49:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [auth].[usuarios](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[creado_en] [datetime2](0) NOT NULL,
	[actualizado_en] [datetime2](0) NULL,
	[correo] [varchar](150) NOT NULL,
	[password_hash] [varchar](255) NOT NULL,
	[nombre] [varchar](100) NOT NULL,
	[apellido] [varchar](100) NOT NULL,
	[telefono] [varchar](20) NULL,
	[rol] [varchar](30) NOT NULL,
	[activo] [bit] NOT NULL,
	[licencia_conducir] [varchar](50) NULL,
	[transmision_habilitada] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [auth].[usuarios] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [auth].[usuarios] ADD  DEFAULT ((1)) FOR [activo]
GO

ALTER TABLE [auth].[usuarios]  WITH NOCHECK ADD  CONSTRAINT [CK_usuarios_rol] CHECK  (([rol]='SUPERADMIN' OR [rol]='ADMINISTRACION' OR [rol]='SOPORTE' OR [rol]='RECEPCION'))
GO

ALTER TABLE [auth].[usuarios] NOCHECK CONSTRAINT [CK_usuarios_rol]
GO



USE [CABS_Pruebas]
GO

/****** Object:  Table [catalog].[clientes]    Script Date: 3/12/2025 10:51:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [catalog].[clientes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[legacy_client_id] [int] NULL,
	[nombre_comercial] [varchar](200) NULL,
	[nombre] [varchar](100) NULL,
	[apellido] [varchar](100) NULL,
	[rfc] [varchar](13) NULL,
	[estado_fiscal] [varchar](50) NULL,
	[telefono] [varchar](20) NULL,
	[email] [varchar](150) NULL,
	[activo] [bit] NOT NULL,
	[created_at] [datetime2](0) NOT NULL,
	[direccion_json] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[legacy_client_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [catalog].[clientes] ADD  DEFAULT ((1)) FOR [activo]
GO

ALTER TABLE [catalog].[clientes] ADD  DEFAULT (getdate()) FOR [created_at]
GO

ALTER TABLE [catalog].[clientes]  WITH NOCHECK ADD  CONSTRAINT [CK_cliente_json] CHECK  (([direccion_json] IS NULL OR isjson([direccion_json])=(1)))
GO

ALTER TABLE [catalog].[clientes] NOCHECK CONSTRAINT [CK_cliente_json]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [catalog].[productos_servicio_ref]    Script Date: 3/12/2025 10:51:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [catalog].[productos_servicio_ref](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](200) NOT NULL,
	[tipo] [varchar](50) NOT NULL,
	[unidad] [varchar](20) NOT NULL,
	[precio_lista] [decimal](12, 2) NOT NULL,
	[legacy_product_id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [catalog].[productos_servicio_ref]  WITH NOCHECK ADD  CONSTRAINT [FK_cat_prod_legacy_prod] FOREIGN KEY([legacy_product_id])
REFERENCES [legacy].[productos_ref] ([legacy_product_id])
GO

ALTER TABLE [catalog].[productos_servicio_ref] NOCHECK CONSTRAINT [FK_cat_prod_legacy_prod]
GO



USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[auth_usuarios]    Script Date: 3/12/2025 10:51:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[auth_usuarios](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[creado_en] [datetime2](0) NOT NULL,
	[actualizado_en] [datetime2](0) NULL,
	[correo] [varchar](150) NOT NULL,
	[password_hash] [varchar](255) NOT NULL,
	[nombre] [varchar](100) NOT NULL,
	[apellido] [varchar](100) NOT NULL,
	[telefono] [bigint] NOT NULL,
	[rol] [varchar](30) NOT NULL,
	[activo] [bit] NOT NULL,
	[transmision_habilitada] [varchar](50) NULL,
	[id_agente_legacy] [int] NULL,
	[codigo_agente_legacy] [nvarchar](30) NULL,
	[nombre_agente_legacy] [nvarchar](60) NULL,
	[fecha_enlace_agente] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[auth_usuarios] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [dbo].[auth_usuarios] ADD  DEFAULT ((1)) FOR [activo]
GO

ALTER TABLE [dbo].[auth_usuarios]  WITH NOCHECK ADD  CONSTRAINT [CK_usuarios_rol] CHECK  (([rol]='ADMINISTRACION' OR [rol]='SOPORTE' OR [rol]='RECEPCION'))
GO

ALTER TABLE [dbo].[auth_usuarios] NOCHECK CONSTRAINT [CK_usuarios_rol]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[calendario_eventos]    Script Date: 3/12/2025 10:51:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[calendario_eventos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[creado_por_id] [int] NOT NULL,
	[titulo] [nvarchar](255) NULL,
	[descripcion] [nvarchar](max) NULL,
	[fecha_inicio] [date] NOT NULL,
	[fecha_fin] [date] NOT NULL,
	[hora_inicio] [time](7) NOT NULL,
	[hora_fin] [time](7) NOT NULL,
	[tipo_evento] [nvarchar](50) NULL,
	[es_compartido] [bit] NOT NULL,
	[color] [nvarchar](50) NULL,
	[fecha_creacion] [datetime2](7) NOT NULL,
	[ultima_modificacion] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[calendario_eventos] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO

ALTER TABLE [dbo].[calendario_eventos] ADD  DEFAULT (getdate()) FOR [ultima_modificacion]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[calendarios_evento_permisos]    Script Date: 3/12/2025 10:51:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[calendarios_evento_permisos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[evento_id] [int] NOT NULL,
	[usuario_id] [int] NOT NULL,
	[tipo_permiso] [nvarchar](50) NULL,
	[fecha_asignacion] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[calendarios_evento_permisos] ADD  DEFAULT (getdate()) FOR [fecha_asignacion]
GO

ALTER TABLE [dbo].[calendarios_evento_permisos]  WITH CHECK ADD  CONSTRAINT [FK_EventoPermisos_Evento] FOREIGN KEY([evento_id])
REFERENCES [dbo].[calendario_eventos] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[calendarios_evento_permisos] CHECK CONSTRAINT [FK_EventoPermisos_Evento]
GO

ALTER TABLE [dbo].[calendarios_evento_permisos]  WITH NOCHECK ADD  CONSTRAINT [FK_EventoPermisos_Usuario] FOREIGN KEY([usuario_id])
REFERENCES [dbo].[auth_usuarios] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[calendarios_evento_permisos] NOCHECK CONSTRAINT [FK_EventoPermisos_Usuario]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[catalog_clientes]    Script Date: 3/12/2025 10:52:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[catalog_clientes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[legacy_client_id] [int] NULL,
	[nombre_comercial] [varchar](200) NULL,
	[nombre] [varchar](100) NULL,
	[apellido] [varchar](100) NULL,
	[rfc] [varchar](13) NULL,
	[estado_fiscal] [varchar](50) NULL,
	[telefono] [varchar](20) NULL,
	[email] [varchar](150) NULL,
	[activo] [bit] NOT NULL,
	[created_at] [datetime2](0) NOT NULL,
	[direccion_json] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[legacy_client_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[catalog_clientes] ADD  DEFAULT ((1)) FOR [activo]
GO

ALTER TABLE [dbo].[catalog_clientes] ADD  DEFAULT (getdate()) FOR [created_at]
GO

ALTER TABLE [dbo].[catalog_clientes]  WITH NOCHECK ADD  CONSTRAINT [FK_clientes_legacy_ref] FOREIGN KEY([legacy_client_id])
REFERENCES [dbo].[legacy_clientes_ref] ([legacy_client_id])
GO

ALTER TABLE [dbo].[catalog_clientes] NOCHECK CONSTRAINT [FK_clientes_legacy_ref]
GO

ALTER TABLE [dbo].[catalog_clientes]  WITH NOCHECK ADD  CONSTRAINT [CK_cliente_direccion_json] CHECK  (([direccion_json] IS NULL OR isjson([direccion_json])=(1)))
GO

ALTER TABLE [dbo].[catalog_clientes] NOCHECK CONSTRAINT [CK_cliente_direccion_json]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[catalog_productos_servicio_ref]    Script Date: 3/12/2025 10:52:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[catalog_productos_servicio_ref](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](200) NOT NULL,
	[tipo] [varchar](50) NOT NULL,
	[unidad] [varchar](20) NOT NULL,
	[precio_lista] [decimal](12, 2) NOT NULL,
	[legacy_product_id] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[catalog_productos_servicio_ref]  WITH NOCHECK ADD  CONSTRAINT [FK_productos_servicio_ref_legacy] FOREIGN KEY([legacy_product_id])
REFERENCES [dbo].[legacy_productos_ref] ([legacy_product_id])
GO

ALTER TABLE [dbo].[catalog_productos_servicio_ref] NOCHECK CONSTRAINT [FK_productos_servicio_ref_legacy]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[evaluacion_detalles]    Script Date: 3/12/2025 11:47:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[evaluacion_detalles](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[evaluacion_id] [int] NOT NULL,
	[fase] [varchar](10) NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[sugerencias] [nvarchar](max) NULL,
	[score_fase] [int] NULL,
	[evidencia_nota] [nvarchar](max) NULL,
	[creado_en] [datetime2](0) NOT NULL,
	[Lugar] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[evaluacion_detalles] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [dbo].[evaluacion_detalles]  WITH NOCHECK ADD  CONSTRAINT [FK_eval_detalle_eval] FOREIGN KEY([evaluacion_id])
REFERENCES [dbo].[evaluaciones] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[evaluacion_detalles] NOCHECK CONSTRAINT [FK_eval_detalle_eval]
GO

ALTER TABLE [dbo].[evaluacion_detalles]  WITH NOCHECK ADD  CONSTRAINT [CK_eval_fase] CHECK  (([fase]='DESPUES' OR [fase]='ANTES'))
GO

ALTER TABLE [dbo].[evaluacion_detalles] NOCHECK CONSTRAINT [CK_eval_fase]
GO

ALTER TABLE [dbo].[evaluacion_detalles]  WITH NOCHECK ADD  CONSTRAINT [CK_eval_score_fase] CHECK  (([score_fase]>=(0) AND [score_fase]<=(100)))
GO

ALTER TABLE [dbo].[evaluacion_detalles] NOCHECK CONSTRAINT [CK_eval_score_fase]
GO



USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[evaluacion_fotos]    Script Date: 3/12/2025 11:48:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[evaluacion_fotos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[detalle_id] [int] NOT NULL,
	[documento_id] [int] NOT NULL,
	[tipo] [varchar](20) NULL,
	[descripcion] [nvarchar](500) NULL,
	[creado_en] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[evaluacion_fotos] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [dbo].[evaluacion_fotos]  WITH NOCHECK ADD  CONSTRAINT [FK_eval_fotos_detalle] FOREIGN KEY([detalle_id])
REFERENCES [dbo].[evaluacion_detalles] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[evaluacion_fotos] NOCHECK CONSTRAINT [FK_eval_fotos_detalle]
GO

ALTER TABLE [dbo].[evaluacion_fotos]  WITH NOCHECK ADD  CONSTRAINT [FK_eval_fotos_documento] FOREIGN KEY([documento_id])
REFERENCES [dbo].[files_documentos] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[evaluacion_fotos] NOCHECK CONSTRAINT [FK_eval_fotos_documento]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[evaluaciones]    Script Date: 3/12/2025 11:48:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[evaluaciones](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orden_id] [int] NOT NULL,
	[ejecucion_id] [int] NULL,
	[cliente_id] [int] NULL,
	[evaluador_id] [int] NOT NULL,
	[objetivo] [nvarchar](200) NULL,
	[comentarios_generales] [nvarchar](max) NULL,
	[score_calidad_total] [int] NULL,
	[requiere_seguimiento] [bit] NOT NULL,
	[seguimiento_notas] [nvarchar](max) NULL,
	[evaluacionActualizada] [nvarchar](255) NULL,
	[nuevaEvaluacion] [nvarchar](255) NULL,
	[creado_en] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[evaluaciones] ADD  DEFAULT ((0)) FOR [requiere_seguimiento]
GO

ALTER TABLE [dbo].[evaluaciones] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [dbo].[evaluaciones]  WITH NOCHECK ADD  CONSTRAINT [FK_eval_orden] FOREIGN KEY([orden_id])
REFERENCES [dbo].[ops_ordenes_trabajo] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[evaluaciones] NOCHECK CONSTRAINT [FK_eval_orden]
GO

ALTER TABLE [dbo].[evaluaciones]  WITH NOCHECK ADD  CONSTRAINT [FK_eval_usuario] FOREIGN KEY([evaluador_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[evaluaciones] NOCHECK CONSTRAINT [FK_eval_usuario]
GO

ALTER TABLE [dbo].[evaluaciones]  WITH NOCHECK ADD  CONSTRAINT [CK_eval_score_total] CHECK  (([score_calidad_total]>=(0) AND [score_calidad_total]<=(100)))
GO

ALTER TABLE [dbo].[evaluaciones] NOCHECK CONSTRAINT [CK_eval_score_total]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[files_documentos]    Script Date: 3/12/2025 11:48:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[files_documentos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[creado_por_usuario_id] [int] NOT NULL,
	[creado_en] [datetime2](0) NOT NULL,
	[entidad_tipo] [varchar](50) NOT NULL,
	[entidad_id] [int] NOT NULL,
	[metadatos_json] [nvarchar](max) NULL,
	[tamano_bytes] [bigint] NULL,
	[mimetype] [varchar](100) NULL,
	[ruta_almacenamiento] [nvarchar](500) NOT NULL,
	[nombre_archivo] [nvarchar](255) NOT NULL,
	[checksum_sha256] [varchar](64) NOT NULL,
	[nombre_original] [nvarchar](255) NOT NULL,
	[activo] [bit] NOT NULL,
	[actualizado_en] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Documento_Entidad] UNIQUE NONCLUSTERED 
(
	[entidad_tipo] ASC,
	[entidad_id] ASC,
	[nombre_archivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[files_documentos] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [dbo].[files_documentos] ADD  DEFAULT ('') FOR [checksum_sha256]
GO

ALTER TABLE [dbo].[files_documentos] ADD  DEFAULT ('') FOR [nombre_original]
GO

ALTER TABLE [dbo].[files_documentos] ADD  DEFAULT ((1)) FOR [activo]
GO

ALTER TABLE [dbo].[files_documentos]  WITH NOCHECK ADD  CONSTRAINT [FK_documentos_usuario] FOREIGN KEY([creado_por_usuario_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[files_documentos] NOCHECK CONSTRAINT [FK_documentos_usuario]
GO

ALTER TABLE [dbo].[files_documentos]  WITH NOCHECK ADD  CONSTRAINT [CK_doc_metadatos_json] CHECK  (([metadatos_json] IS NULL OR isjson([metadatos_json])=(1)))
GO

ALTER TABLE [dbo].[files_documentos] NOCHECK CONSTRAINT [CK_doc_metadatos_json]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[finance_gastos_viaticos]    Script Date: 3/12/2025 11:48:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[finance_gastos_viaticos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orden_id] [int] NOT NULL,
	[tiene_factura] [bit] NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[proveedor_nombre] [varchar](200) NULL,
	[fecha] [date] NOT NULL,
	[km_recorridos] [int] NULL,
	[gastos] [varchar](200) NOT NULL,
	[monto_total] [decimal](12, 2) NOT NULL,
	[lugar_destino] [varchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[finance_gastos_viaticos] ADD  DEFAULT ((0)) FOR [tiene_factura]
GO

ALTER TABLE [dbo].[finance_gastos_viaticos]  WITH NOCHECK ADD  CONSTRAINT [FK_gastos_viaticos_orden] FOREIGN KEY([orden_id])
REFERENCES [dbo].[ops_ordenes_trabajo] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[finance_gastos_viaticos] NOCHECK CONSTRAINT [FK_gastos_viaticos_orden]
GO

ALTER TABLE [dbo].[finance_gastos_viaticos]  WITH NOCHECK ADD  CONSTRAINT [CK_viatico_km] CHECK  (([km_recorridos] IS NULL OR [km_recorridos]>=(0)))
GO

ALTER TABLE [dbo].[finance_gastos_viaticos] NOCHECK CONSTRAINT [CK_viatico_km]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[fleet_uso_vehiculos]    Script Date: 3/12/2025 11:48:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[fleet_uso_vehiculos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[vehiculo_id] [int] NOT NULL,
	[usuario_id] [int] NOT NULL,
	[fecha] [date] NOT NULL,
	[hora_salida] [time](7) NOT NULL,
	[hora_regreso] [time](7) NULL,
	[motivo_uso] [nvarchar](500) NOT NULL,
	[kilometraje_inicial] [int] NOT NULL,
	[kilometraje_final] [int] NULL,
	[observaciones] [nvarchar](max) NULL,
	[firma_base64] [nvarchar](max) NULL,
	[estado] [varchar](20) NOT NULL,
	[fecha_creacion] [datetime2](7) NOT NULL,
	[fecha_actualizacion] [datetime2](7) NULL,
 CONSTRAINT [PK_fleet_uso_vehiculos] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos] ADD  DEFAULT ('EN_USO') FOR [estado]
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos]  WITH CHECK ADD  CONSTRAINT [FK_fleet_uso_vehiculos_usuario] FOREIGN KEY([usuario_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos] CHECK CONSTRAINT [FK_fleet_uso_vehiculos_usuario]
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos]  WITH CHECK ADD  CONSTRAINT [FK_fleet_uso_vehiculos_vehiculo] FOREIGN KEY([vehiculo_id])
REFERENCES [dbo].[fleet_vehiculos] ([id])
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos] CHECK CONSTRAINT [FK_fleet_uso_vehiculos_vehiculo]
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos]  WITH CHECK ADD  CONSTRAINT [CK_fleet_uso_vehiculos_estado] CHECK  (([estado]='CANCELADO' OR [estado]='COMPLETADO' OR [estado]='EN_USO'))
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos] CHECK CONSTRAINT [CK_fleet_uso_vehiculos_estado]
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos]  WITH CHECK ADD  CONSTRAINT [CK_fleet_uso_vehiculos_kilometraje] CHECK  (([kilometraje_final] IS NULL OR [kilometraje_final]>=[kilometraje_inicial]))
GO

ALTER TABLE [dbo].[fleet_uso_vehiculos] CHECK CONSTRAINT [CK_fleet_uso_vehiculos_kilometraje]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[fleet_vehiculos]    Script Date: 3/12/2025 11:49:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[fleet_vehiculos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tipo_vehiculo] [varchar](50) NULL,
	[transmision] [varchar](20) NULL,
	[es_de_empresa] [bit] NOT NULL,
	[placas] [varchar](20) NULL,
	[activo] [bit] NOT NULL,
	[observaciones] [nvarchar](max) NULL,
	[nombre_vehiculo] [varchar](100) NOT NULL,
	[kilometraje] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[placas] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[fleet_vehiculos] ADD  DEFAULT ((1)) FOR [es_de_empresa]
GO

ALTER TABLE [dbo].[fleet_vehiculos] ADD  DEFAULT ((1)) FOR [activo]
GO

ALTER TABLE [dbo].[fleet_vehiculos] ADD  DEFAULT ('') FOR [nombre_vehiculo]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[legacy_clientes_ref]    Script Date: 3/12/2025 11:49:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[legacy_clientes_ref](
	[legacy_client_id] [int] NOT NULL,
	[last_sync_timestamp] [datetime2](0) NULL,
PRIMARY KEY CLUSTERED 
(
	[legacy_client_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[legacy_intakes_ref]    Script Date: 3/12/2025 11:49:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[legacy_intakes_ref](
	[legacy_intake_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[legacy_intake_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[legacy_productos_ref]    Script Date: 3/12/2025 11:49:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[legacy_productos_ref](
	[legacy_product_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[legacy_product_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[notificaciones]    Script Date: 3/12/2025 11:49:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[notificaciones](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[usuario_id] [int] NOT NULL,
	[tipo] [varchar](50) NOT NULL,
	[titulo] [varchar](200) NOT NULL,
	[mensaje] [varchar](1000) NOT NULL,
	[datos] [nvarchar](max) NULL,
	[leida] [bit] NOT NULL,
	[fecha_creacion] [datetime2](0) NOT NULL,
	[fecha_lectura] [datetime2](0) NULL,
	[prioridad] [varchar](20) NOT NULL,
	[accion] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[notificaciones] ADD  DEFAULT ((0)) FOR [leida]
GO

ALTER TABLE [dbo].[notificaciones] ADD  DEFAULT (sysutcdatetime()) FOR [fecha_creacion]
GO

ALTER TABLE [dbo].[notificaciones] ADD  DEFAULT ('MEDIA') FOR [prioridad]
GO

ALTER TABLE [dbo].[notificaciones]  WITH NOCHECK ADD  CONSTRAINT [FK_notificaciones_usuario] FOREIGN KEY([usuario_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[notificaciones] NOCHECK CONSTRAINT [FK_notificaciones_usuario]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[ops_ejecuciones_orden]    Script Date: 3/12/2025 11:49:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ops_ejecuciones_orden](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orden_id] [int] NOT NULL,
	[tipo_ejecucion] [varchar](20) NOT NULL,
	[tecnico_id] [int] NOT NULL,
	[hr_inicio] [datetime2](0) NULL,
	[hr_fin] [datetime2](0) NULL,
	[comentarios] [nvarchar](max) NULL,
	[vehiculo_id] [int] NULL,
	[km_inicial] [int] NULL,
	[km_final] [int] NULL,
	[herramientas] [varchar](100) NULL,
	[codigo_sesion] [varchar](100) NULL,
	[contrasena_sesion] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [FK_ejecuciones_orden_orden] FOREIGN KEY([orden_id])
REFERENCES [dbo].[ops_ordenes_trabajo] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden] NOCHECK CONSTRAINT [FK_ejecuciones_orden_orden]
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [FK_ejecuciones_orden_tecnico] FOREIGN KEY([tecnico_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden] NOCHECK CONSTRAINT [FK_ejecuciones_orden_tecnico]
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [FK_ejecuciones_orden_vehiculo] FOREIGN KEY([vehiculo_id])
REFERENCES [dbo].[fleet_vehiculos] ([id])
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden] NOCHECK CONSTRAINT [FK_ejecuciones_orden_vehiculo]
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [CK_ejecucion_tipo] CHECK  (([tipo_ejecucion]='CAMPO' OR [tipo_ejecucion]='REMOTO'))
GO

ALTER TABLE [dbo].[ops_ejecuciones_orden] NOCHECK CONSTRAINT [CK_ejecucion_tipo]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[ops_ordenes_trabajo]    Script Date: 3/12/2025 11:50:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ops_ordenes_trabajo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nuevo_cliente] [bit] NULL,
	[nombre_cliente] [varchar](120) NULL,
	[cliente_id] [int] NULL,
	[creado_por_user_id] [int] NOT NULL,
	[asignada_a_user_id] [int] NULL,
	[notas] [nvarchar](max) NULL,
	[cita_programada_inicio] [datetime2](0) NULL,
	[cita_programada_fin] [datetime2](0) NULL,
	[modalidad] [varchar](50) NULL,
	[tipo_orden] [varchar](50) NULL,
	[prioridad] [int] NULL,
	[estado] [varchar](20) NOT NULL,
	[ubicacion_text] [nvarchar](max) NULL,
	[requiere_factura] [bit] NOT NULL,
	[estado_facturado] [varchar](50) NULL,
	[factura_folio] [varchar](50) NULL,
	[creado_en] [datetime2](0) NOT NULL,
	[actualizado_en] [datetime2](0) NULL,
	[costo_real] [decimal](12, 2) NULL,
	[costo_estimado] [decimal](12, 2) NULL,
	[cliente_telefono] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo] ADD  DEFAULT ((0)) FOR [requiere_factura]
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo]  WITH NOCHECK ADD  CONSTRAINT [FK_ordenes_trabajo_asignada_a] FOREIGN KEY([asignada_a_user_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo] NOCHECK CONSTRAINT [FK_ordenes_trabajo_asignada_a]
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo]  WITH NOCHECK ADD  CONSTRAINT [FK_ordenes_trabajo_cliente] FOREIGN KEY([cliente_id])
REFERENCES [dbo].[catalog_clientes] ([id])
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo] NOCHECK CONSTRAINT [FK_ordenes_trabajo_cliente]
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo]  WITH NOCHECK ADD  CONSTRAINT [FK_ordenes_trabajo_creado_por] FOREIGN KEY([creado_por_user_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo] NOCHECK CONSTRAINT [FK_ordenes_trabajo_creado_por]
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo]  WITH NOCHECK ADD  CONSTRAINT [CK_orden_estado] CHECK  (([estado]='CERRADA' OR [estado]='FACTURADA' OR [estado]='POR_FACTURAR' OR [estado]='COMPLETADA' OR [estado]='EN_CURSO' OR [estado]='ASIGNADA' OR [estado]='CAPTURADA'))
GO

ALTER TABLE [dbo].[ops_ordenes_trabajo] NOCHECK CONSTRAINT [CK_orden_estado]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[RecuperacionPasswordToken]    Script Date: 3/12/2025 11:50:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RecuperacionPasswordToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](256) NULL,
	[Token] [nvarchar](6) NULL,
	[Expiracion] [datetime] NULL,
	[Usado] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RecuperacionPasswordToken] ADD  DEFAULT ((0)) FOR [Usado]
GO

USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[RecuperacionPasswordTokens]    Script Date: 3/12/2025 11:50:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RecuperacionPasswordTokens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Token] [nvarchar](10) NOT NULL,
	[Expiracion] [datetime2](7) NOT NULL,
	[Usado] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[RecuperacionPasswordTokens] ADD  DEFAULT ((0)) FOR [Usado]
GO

ALTER TABLE [dbo].[RecuperacionPasswordTokens] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[reparacion_componentes]    Script Date: 3/12/2025 11:50:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[reparacion_componentes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[reparacion_id] [int] NOT NULL,
	[componente] [varchar](160) NOT NULL,
	[cantidad] [int] NOT NULL,
	[proveedor] [varchar](160) NULL,
	[garantia_meses] [int] NULL,
	[costo_unitario_compra] [decimal](12, 2) NULL,
	[costo_unitario_publico] [decimal](12, 2) NULL,
	[subtotal_compra]  AS (isnull([cantidad],(0))*isnull([costo_unitario_compra],(0))) PERSISTED,
	[subtotal_publico]  AS (isnull([cantidad],(0))*isnull([costo_unitario_publico],(0))) PERSISTED,
	[notas] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[reparacion_componentes] ADD  DEFAULT ((1)) FOR [cantidad]
GO

ALTER TABLE [dbo].[reparacion_componentes]  WITH NOCHECK ADD  CONSTRAINT [FK_rep_comp_rep] FOREIGN KEY([reparacion_id])
REFERENCES [dbo].[reparaciones] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[reparacion_componentes] NOCHECK CONSTRAINT [FK_rep_comp_rep]
GO

ALTER TABLE [dbo].[reparacion_componentes]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_comp_cantidad] CHECK  (([cantidad]>(0)))
GO

ALTER TABLE [dbo].[reparacion_componentes] NOCHECK CONSTRAINT [CK_rep_comp_cantidad]
GO

ALTER TABLE [dbo].[reparacion_componentes]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_comp_costo_compra] CHECK  (([costo_unitario_compra] IS NULL OR [costo_unitario_compra]>=(0)))
GO

ALTER TABLE [dbo].[reparacion_componentes] NOCHECK CONSTRAINT [CK_rep_comp_costo_compra]
GO

ALTER TABLE [dbo].[reparacion_componentes]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_comp_costo_publico] CHECK  (([costo_unitario_publico] IS NULL OR [costo_unitario_publico]>=(0)))
GO

ALTER TABLE [dbo].[reparacion_componentes] NOCHECK CONSTRAINT [CK_rep_comp_costo_publico]
GO

ALTER TABLE [dbo].[reparacion_componentes]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_comp_garantia] CHECK  (([garantia_meses] IS NULL OR [garantia_meses]>=(0)))
GO

ALTER TABLE [dbo].[reparacion_componentes] NOCHECK CONSTRAINT [CK_rep_comp_garantia]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[reparacion_fotos]    Script Date: 3/12/2025 11:50:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[reparacion_fotos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[reparacion_id] [int] NOT NULL,
	[documento_id] [int] NOT NULL,
	[etapa] [varchar](20) NULL,
	[descripcion] [nvarchar](500) NULL,
	[creado_en] [datetime2](0) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[reparacion_fotos] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [dbo].[reparacion_fotos]  WITH NOCHECK ADD  CONSTRAINT [FK_rep_fotos_documento] FOREIGN KEY([documento_id])
REFERENCES [dbo].[files_documentos] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[reparacion_fotos] NOCHECK CONSTRAINT [FK_rep_fotos_documento]
GO

ALTER TABLE [dbo].[reparacion_fotos]  WITH NOCHECK ADD  CONSTRAINT [FK_rep_fotos_rep] FOREIGN KEY([reparacion_id])
REFERENCES [dbo].[reparaciones] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[reparacion_fotos] NOCHECK CONSTRAINT [FK_rep_fotos_rep]
GO

USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[reparaciones]    Script Date: 3/12/2025 11:51:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[reparaciones](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orden_id] [int] NOT NULL,
	[tecnico_id] [int] NOT NULL,
	[dispositivo_tipo] [varchar](80) NOT NULL,
	[marca] [varchar](80) NULL,
	[modelo] [varchar](120) NULL,
	[accesorios_recibidos] [nvarchar](500) NULL,
	[descripcion_falla] [nvarchar](max) NOT NULL,
	[diagnostico] [nvarchar](max) NULL,
	[solucion_aplicada] [nvarchar](max) NULL,
	[resultado] [varchar](20) NOT NULL,
	[causa_irreparable] [nvarchar](max) NULL,
	[respaldo_datos_autorizado] [bit] NOT NULL,
	[costo_mano_obra] [decimal](12, 2) NULL,
	[costo_refacciones_compra] [decimal](12, 2) NULL,
	[costo_refacciones_publico] [decimal](12, 2) NULL,
	[costo_total_compra]  AS (isnull([costo_mano_obra],(0))+isnull([costo_refacciones_compra],(0))) PERSISTED,
	[costo_total_publico]  AS (isnull([costo_mano_obra],(0))+isnull([costo_refacciones_publico],(0))) PERSISTED,
	[margen_estimado]  AS (isnull([costo_refacciones_publico],(0))-isnull([costo_refacciones_compra],(0))) PERSISTED,
	[garantia_dias] [int] NULL,
	[fecha_llegada] [datetime2](0) NOT NULL,
	[empezado_en] [datetime2](0) NULL,
	[entregado_en] [datetime2](0) NULL,
	[tipo_entrega] [varchar](20) NOT NULL,
	[ubicacion_almacenamiento] [nvarchar](200) NULL,
	[notas] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[reparaciones] ADD  DEFAULT ((0)) FOR [respaldo_datos_autorizado]
GO

ALTER TABLE [dbo].[reparaciones] ADD  DEFAULT (getdate()) FOR [fecha_llegada]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [FK_rep_orden] FOREIGN KEY([orden_id])
REFERENCES [dbo].[ops_ordenes_trabajo] ([id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [FK_rep_orden]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [FK_rep_tecnico] FOREIGN KEY([tecnico_id])
REFERENCES [dbo].[auth_usuarios] ([id])
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [FK_rep_tecnico]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_garantia] CHECK  (([garantia_dias] IS NULL OR [garantia_dias]>=(0)))
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [CK_rep_garantia]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_mano_obra] CHECK  (([costo_mano_obra] IS NULL OR [costo_mano_obra]>=(0)))
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [CK_rep_mano_obra]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_refacciones_compra] CHECK  (([costo_refacciones_compra] IS NULL OR [costo_refacciones_compra]>=(0)))
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [CK_rep_refacciones_compra]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_refacciones_publico] CHECK  (([costo_refacciones_publico] IS NULL OR [costo_refacciones_publico]>=(0)))
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [CK_rep_refacciones_publico]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [CK_rep_tipo_entrega] CHECK  (([tipo_entrega]='RECOGE_CLIENTE' OR [tipo_entrega]='DOMICILIO'))
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [CK_rep_tipo_entrega]
GO

ALTER TABLE [dbo].[reparaciones]  WITH NOCHECK ADD  CONSTRAINT [CK_reparacion_resultado] CHECK  (([resultado]='DEVUELTO_SIN_REPARAR' OR [resultado]='COTIZAR' OR [resultado]='IRREPARABLE' OR [resultado]='REPARADO'))
GO

ALTER TABLE [dbo].[reparaciones] NOCHECK CONSTRAINT [CK_reparacion_resultado]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[sales_cotizaciones]    Script Date: 3/12/2025 11:51:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[sales_cotizaciones](
	[CIDDOCUMENTO] [int] IDENTITY(1,1) NOT NULL,
	[CIDDOCUMENTODE] [int] NOT NULL,
	[CIDCONCEPTODOCUMENTO] [int] NOT NULL,
	[CIDCLIENTEPROVEEDOR] [int] NOT NULL,
	[CIDAGENTE] [int] NOT NULL,
	[CIDDOCUMENTOORIGEN] [int] NULL,
	[CSERIEDOCUMENTO] [varchar](11) NULL,
	[CFOLIO] [float] NOT NULL,
	[CFECHA] [datetime] NOT NULL,
	[CFECHAVENCIMIENTO] [datetime] NULL,
	[CFECHAENTREGARECEPCION] [datetime] NULL,
	[CRAZONSOCIAL] [varchar](60) NULL,
	[CRFC] [varchar](20) NULL,
	[CREFERENCIA] [varchar](20) NULL,
	[COBSERVACIONES] [varchar](max) NULL,
	[CNATURALEZA] [int] NOT NULL,
	[CUSACLIENTE] [int] NOT NULL,
	[CAFECTADO] [int] NOT NULL,
	[CIMPRESO] [int] NOT NULL,
	[CCANCELADO] [int] NOT NULL,
	[CNETO] [float] NOT NULL,
	[CIMPUESTO1] [float] NOT NULL,
	[CDESCUENTOMOV] [float] NOT NULL,
	[CTOTAL] [float] NOT NULL,
	[CPENDIENTE] [float] NOT NULL,
	[CTOTALUNIDADES] [float] NOT NULL,
 CONSTRAINT [PK_sales_cotizaciones] PRIMARY KEY CLUSTERED 
(
	[CIDDOCUMENTO] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CFOLIO]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT (getdate()) FOR [CFECHA]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CNATURALEZA]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((1)) FOR [CUSACLIENTE]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CAFECTADO]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CIMPRESO]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CCANCELADO]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CNETO]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CIMPUESTO1]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CDESCUENTOMOV]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CTOTAL]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CPENDIENTE]
GO

ALTER TABLE [dbo].[sales_cotizaciones] ADD  DEFAULT ((0)) FOR [CTOTALUNIDADES]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [dbo].[usuario_horarios]    Script Date: 3/12/2025 11:51:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[usuario_horarios](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UsuarioId] [int] NOT NULL,
	[Nombre] [nvarchar](100) NOT NULL,
	[DiaInicio] [nvarchar](20) NOT NULL,
	[DiaFin] [nvarchar](20) NOT NULL,
	[HoraInicio] [time](7) NOT NULL,
	[HoraFin] [time](7) NOT NULL,
	[IsCompartido] [bit] NOT NULL,
	[CreadoEn] [datetime2](7) NOT NULL,
	[ActualizadoEn] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[usuario_horarios] ADD  DEFAULT ((0)) FOR [IsCompartido]
GO

ALTER TABLE [dbo].[usuario_horarios] ADD  DEFAULT (getutcdate()) FOR [CreadoEn]
GO

ALTER TABLE [dbo].[usuario_horarios]  WITH NOCHECK ADD  CONSTRAINT [FK_usuario_horarios_auth_usuarios] FOREIGN KEY([UsuarioId])
REFERENCES [dbo].[auth_usuarios] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[usuario_horarios] NOCHECK CONSTRAINT [FK_usuario_horarios_auth_usuarios]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [files].[documentos]    Script Date: 3/12/2025 11:51:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [files].[documentos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[creado_por_usuario_id] [int] NOT NULL,
	[creado_en] [datetime2](0) NOT NULL,
	[entidad_tipo] [varchar](50) NOT NULL,
	[entidad_id] [int] NOT NULL,
	[metadatos_json] [nvarchar](max) NULL,
	[tamano_bytes] [bigint] NULL,
	[mimetype] [varchar](100) NULL,
	[ruta_almacenamiento] [nvarchar](500) NOT NULL,
	[nombre_archivo] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Documento_Entidad] UNIQUE NONCLUSTERED 
(
	[entidad_tipo] ASC,
	[entidad_id] ASC,
	[nombre_archivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [files].[documentos] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [files].[documentos]  WITH NOCHECK ADD  CONSTRAINT [FK_doc_usuario] FOREIGN KEY([creado_por_usuario_id])
REFERENCES [auth].[usuarios] ([id])
ON UPDATE CASCADE
GO

ALTER TABLE [files].[documentos] NOCHECK CONSTRAINT [FK_doc_usuario]
GO

ALTER TABLE [files].[documentos]  WITH NOCHECK ADD  CONSTRAINT [CK_doc_metadatos_json] CHECK  (([metadatos_json] IS NULL OR isjson([metadatos_json])=(1)))
GO

ALTER TABLE [files].[documentos] NOCHECK CONSTRAINT [CK_doc_metadatos_json]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [finance].[gastos_viaticos]    Script Date: 3/12/2025 11:51:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [finance].[gastos_viaticos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orden_id] [int] NOT NULL,
	[tiene_factura] [bit] NOT NULL,
	[descripcion] [nvarchar](max) NULL,
	[proveedor_nombre] [varchar](200) NULL,
	[fecha] [date] NOT NULL,
	[km_recorridos] [int] NULL,
	[gastos] [varchar](200) NOT NULL,
	[monto_total] [decimal](12, 2) NOT NULL,
	[lugar_destino] [varchar](200) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [finance].[gastos_viaticos] ADD  DEFAULT ((0)) FOR [tiene_factura]
GO

ALTER TABLE [finance].[gastos_viaticos]  WITH NOCHECK ADD  CONSTRAINT [FK_viatico_orden] FOREIGN KEY([orden_id])
REFERENCES [ops].[ordenes_trabajo] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [finance].[gastos_viaticos] NOCHECK CONSTRAINT [FK_viatico_orden]
GO

ALTER TABLE [finance].[gastos_viaticos]  WITH NOCHECK ADD  CONSTRAINT [CK_viatico_km] CHECK  (([km_recorridos] IS NULL OR [km_recorridos]>=(0)))
GO

ALTER TABLE [finance].[gastos_viaticos] NOCHECK CONSTRAINT [CK_viatico_km]
GO

USE [CABS_Pruebas]
GO

/****** Object:  Table [fleet].[vehiculos]    Script Date: 3/12/2025 11:51:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [fleet].[vehiculos](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tipo_vehiculo] [varchar](50) NULL,
	[transmision] [varchar](20) NULL,
	[es_de_empresa] [bit] NOT NULL,
	[placas] [varchar](20) NULL,
	[activo] [bit] NOT NULL,
	[observaciones] [nvarchar](max) NULL,
	[kilometraje_actual] [decimal](10, 2) NULL,
	[ultimo_uso] [datetime2](0) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[placas] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [fleet].[vehiculos] ADD  DEFAULT ((1)) FOR [es_de_empresa]
GO

ALTER TABLE [fleet].[vehiculos] ADD  DEFAULT ((1)) FOR [activo]
GO

ALTER TABLE [fleet].[vehiculos] ADD  DEFAULT ((0)) FOR [kilometraje_actual]
GO

USE [CABS_Pruebas]
GO

/****** Object:  Table [legacy].[clientes_ref]    Script Date: 3/12/2025 11:51:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [legacy].[clientes_ref](
	[legacy_client_id] [int] NOT NULL,
	[last_sync_timestamp] [datetime2](0) NULL,
PRIMARY KEY CLUSTERED 
(
	[legacy_client_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [legacy].[intakes_ref]    Script Date: 3/12/2025 11:52:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [legacy].[intakes_ref](
	[legacy_intake_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[legacy_intake_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [legacy].[productos_ref]    Script Date: 3/12/2025 11:52:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [legacy].[productos_ref](
	[legacy_product_id] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[legacy_product_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


USE [CABS_Pruebas]
GO

/****** Object:  Table [ops].[ejecuciones_orden]    Script Date: 3/12/2025 11:52:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ops].[ejecuciones_orden](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[orden_id] [int] NOT NULL,
	[tipo_ejecucion] [varchar](20) NOT NULL,
	[tecnico_id] [int] NOT NULL,
	[hr_inicio] [datetime2](0) NULL,
	[hr_fin] [datetime2](0) NULL,
	[comentarios] [nvarchar](max) NULL,
	[vehiculo_id] [int] NULL,
	[km_inicial] [int] NULL,
	[km_final] [int] NULL,
	[herramientas] [varchar](100) NULL,
	[codigo_sesion] [varchar](100) NULL,
	[contrasena_sesion] [varchar](100) NULL,
	[evaluacionActualizada] [nvarchar](255) NULL,
	[nuevaEvaluacion] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [ops].[ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [FK_ejecucion_orden] FOREIGN KEY([orden_id])
REFERENCES [ops].[ordenes_trabajo] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [ops].[ejecuciones_orden] NOCHECK CONSTRAINT [FK_ejecucion_orden]
GO

ALTER TABLE [ops].[ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [FK_ejecucion_tecnico] FOREIGN KEY([tecnico_id])
REFERENCES [auth].[usuarios] ([id])
GO

ALTER TABLE [ops].[ejecuciones_orden] NOCHECK CONSTRAINT [FK_ejecucion_tecnico]
GO

ALTER TABLE [ops].[ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [FK_ejecucion_vehiculo] FOREIGN KEY([vehiculo_id])
REFERENCES [fleet].[vehiculos] ([id])
GO

ALTER TABLE [ops].[ejecuciones_orden] NOCHECK CONSTRAINT [FK_ejecucion_vehiculo]
GO

ALTER TABLE [ops].[ejecuciones_orden]  WITH NOCHECK ADD  CONSTRAINT [CK_ejecucion_tipo] CHECK  (([tipo_ejecucion]='CAMPO' OR [tipo_ejecucion]='REMOTO'))
GO

ALTER TABLE [ops].[ejecuciones_orden] NOCHECK CONSTRAINT [CK_ejecucion_tipo]
GO

USE [CABS_Pruebas]
GO

/****** Object:  Table [ops].[ordenes_trabajo]    Script Date: 3/12/2025 11:52:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ops].[ordenes_trabajo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[cliente_id] [int] NOT NULL,
	[creado_por_user_id] [int] NOT NULL,
	[asignada_a_user_id] [int] NULL,
	[notas] [nvarchar](max) NULL,
	[cita_programada_inicio] [datetime2](0) NULL,
	[cita_programada_fin] [datetime2](0) NULL,
	[modalidad] [varchar](50) NULL,
	[tipo_orden] [varchar](50) NULL,
	[prioridad] [int] NULL,
	[estado] [varchar](20) NOT NULL,
	[ubicacion_text] [nvarchar](max) NULL,
	[requiere_factura] [bit] NOT NULL,
	[estado_facturado] [varchar](50) NULL,
	[factura_folio] [varchar](50) NULL,
	[creado_en] [datetime2](0) NOT NULL,
	[actualizado_en] [datetime2](0) NULL,
	[costo_real] [decimal](12, 2) NULL,
	[costo_estimado] [decimal](12, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [ops].[ordenes_trabajo] ADD  DEFAULT ((0)) FOR [requiere_factura]
GO

ALTER TABLE [ops].[ordenes_trabajo] ADD  DEFAULT (getdate()) FOR [creado_en]
GO

ALTER TABLE [ops].[ordenes_trabajo]  WITH NOCHECK ADD  CONSTRAINT [FK_orden_asignada_a] FOREIGN KEY([asignada_a_user_id])
REFERENCES [auth].[usuarios] ([id])
ON UPDATE CASCADE
GO

ALTER TABLE [ops].[ordenes_trabajo] NOCHECK CONSTRAINT [FK_orden_asignada_a]
GO

ALTER TABLE [ops].[ordenes_trabajo]  WITH NOCHECK ADD  CONSTRAINT [FK_orden_cliente] FOREIGN KEY([cliente_id])
REFERENCES [catalog].[clientes] ([id])
GO

ALTER TABLE [ops].[ordenes_trabajo] NOCHECK CONSTRAINT [FK_orden_cliente]
GO

ALTER TABLE [ops].[ordenes_trabajo]  WITH NOCHECK ADD  CONSTRAINT [CK_orden_estado] CHECK  (([estado]='CERRADA' OR [estado]='FACTURADA' OR [estado]='POR_FACTURAR' OR [estado]='COMPLETADA' OR [estado]='EN_CURSO' OR [estado]='ASIGNADA' OR [estado]='CAPTURADA'))
GO

ALTER TABLE [ops].[ordenes_trabajo] NOCHECK CONSTRAINT [CK_orden_estado]
GO

