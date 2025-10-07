// =====================================================================================
// DTO CLIENTES COMPLETOS - VwClientesCompletosDto.cs
// =====================================================================================
//
// ¿QUÉ HACE ESTE ARCHIVO?
// Define el Objeto de Transferencia de Datos (DTO) para la vista VwClientesCompletos.
// Se utiliza para exponer los datos de la vista a través de la API de una manera
// controlada y desacoplada del modelo de base de datos.
//
// CÓMO USARLO:
// El servicio mapeará la entidad VwClientesCompletos a este DTO antes de devolver
// los datos al controlador y, finalmente, al cliente.
//
// =====================================================================================

namespace back_cabs.CRM.DTOs
{
    /// <summary>
    /// DTO para la vista de solo lectura VwClientesCompletos.
    /// </summary>
    public class VwClientesCompletosDto
    {
        public int ClienteId { get; set; }
        public string? NombreComercial { get; set; }
        public string? RFC { get; set; }
        public bool Activo { get; set; }
        public int? LegacyClientId { get; set; }
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? Colonia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        public string? Pais { get; set; }
        public string? TelefonoPrincipal { get; set; }
        public string? EmailPrincipal { get; set; }
    }
}
