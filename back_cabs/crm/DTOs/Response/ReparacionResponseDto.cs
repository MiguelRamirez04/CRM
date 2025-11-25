namespace back_cabs.CRM.DTOs.Soporte
{
    /// <summary>
    /// DTO para la respuesta de una Reparación (usado al consultar o después de crear/actualizar).
    /// Contiene la información completa, incluyendo IDs generados y costos calculados.
    /// </summary>
    public record ReparacionResponseDto
    {
        // IDENTIFICACIÓN Y RELACIONES CLAVE
        public int Id { get; init; } // ID generado por la DB
        public int OrdenId { get; init; }
        public int TecnicoId { get; init; }

        // INFORMACIÓN DEL DISPOSITIVO
        public string DispositivoTipo { get; init; } = string.Empty;
        public string? Marca { get; init; }
        public string? Modelo { get; init; }
        public string? AccesoriosRecibidos { get; init; }
        public string DescripcionFalla { get; init; } = string.Empty;

        // ESTADO Y RESULTADO DEL TRABAJO
        public string? Diagnostico { get; init; }
        public string? SolucionAplicada { get; init; }
        public string Resultado { get; init; } = string.Empty;
        public string? CausaIrreparable { get; init; }
        public bool RespaldoDatosAutorizado { get; init; }
        public int? GarantiaDias { get; init; }

        // LOGÍSTICA DE TIEMPO
        public DateTime FechaLlegada { get; init; }
        public DateTime? EmpezadoEn { get; init; }
        public DateTime? EntregadoEn { get; init; }
        public string TipoEntrega { get; init; } = string.Empty;
        public string? UbicacionAlmacenamiento { get; init; }
        public string? Notas { get; init; }

        // COSTOS (CALCULADOS)
        // Se incluyen los costos de compra y mano de obra para transparencia
        public decimal? CostoManoObra { get; init; }
        public decimal? CostoRefaccionesCompra { get; init; }
        public decimal? CostoRefaccionesPublico { get; init; }

        // Se exponen los totales calculados por la DB (o por el servicio)
        public decimal CostoTotalCompra { get; init; }
        public decimal CostoTotalPublico { get; init; }
        public decimal MargenEstimado { get; init; }

        // OPCIONAL: Nombres para facilitar la lectura al cliente
        public string NombreTecnico { get; set; } = string.Empty;
        public string FolioOrden { get; set; } = string.Empty;

        public string? NombreCliente { get; set; }
        public long Telefono { get; set; }
    }

    // =====================================================================================
    // DTO REPARACIÓN COMPONENTES- ReparacionComponenteResponseDto
    // =====================================================================================

    public record ReparacionComponenteResponseDto
    {
        public int Id { get; init; }
        public int ReparacionId { get; init; }
        public string? Componente { get; init; }
        public int Cantidad { get; init; }
        public string? Proveedor { get; init; }
        public int? GarantiaMeses { get; init; }
        public decimal? CostoUnitarioCompra { get; init; }
        public decimal? CostoUnitarioPublico { get; init; }
        public decimal SubtotalCompra { get; init; }
        public decimal SubtotalPublico { get; init; }
        public string? Notas { get; init; }

    }
}