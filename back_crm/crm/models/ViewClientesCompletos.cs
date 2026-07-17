//file: back_cabs/CRM/models/ViewClientesCompletos.cs

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace back_cabs.CRM.models;

/// <summary>
/// Modelo para la vista de solo lectura VwClientesCompletos
/// Actualizado según los errores reportados en los logs
/// </summary>
[Keyless]
[Table("VwClientesCompletos")]
public class VwClientesCompletos
{
    [Column("id")]
    public int ClienteId { get; set; }
    
    [Column("nombre")]
    public string? NombreComercial { get; set; }
    
    [Column("rfc")]
    public string? RFC { get; set; }
    
    [Column("activo")]
    public bool Activo { get; set; }
    
    [Column("id_cliente_legacy")]
    public int? LegacyClientId { get; set; }
    
    [Column("calle")]
    public string? Calle { get; set; }
    
    [Column("numero")]
    public string? NumeroExterior { get; set; }
    
    [Column("colonia")]
    public string? Colonia { get; set; }
    
    [Column("cp")]
    public string? CodigoPostal { get; set; }
    
    [Column("ciudad")]
    public string? Ciudad { get; set; }
    
    [Column("estado")]
    public string? Estado { get; set; }
    
    [Column("pais")]
    public string? Pais { get; set; }
    
    [Column("telefono")]
    public string? TelefonoPrincipal { get; set; }
    
    [Column("email")]
    public string? EmailPrincipal { get; set; }
}