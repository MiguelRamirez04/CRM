using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.models;

/// <summary>
/// Modelo para la vista de solo lectura VwClientesCompletos
/// </summary>
[Keyless]
public class VwClientesCompletos
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