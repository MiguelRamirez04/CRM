namespace back_cabs.CRM.enums.Files;

/// <summary>
/// Tipos de entidades que pueden tener archivos adjuntos
/// </summary>
public enum TipoEntidadDocumento
{
    /// <summary>
    /// Fotografía de evaluación (solo imágenes, se convierte a WebP)
    /// </summary>
    Evaluacion,

    /// <summary>
    /// Fotografía de reparación (solo imágenes, se convierte a WebP)
    /// </summary>
    Reparacion,

    /// <summary>
    /// Gasto de viáticos (facturas PDF/Excel)
    /// </summary>
    GastoViatico,

    /// <summary>
    /// Orden de trabajo (cotizaciones PDF/Excel)
    /// </summary>
    OrdenTrabajo,

    /// <summary>
    /// Cliente (documentos generales)
    /// </summary>
    Cliente,

    /// <summary>
    /// Vehículo (documentos generales)
    /// </summary>
    Vehiculo
}

/// <summary>
/// Categorías de documentos
/// </summary>
public enum CategoriaDocumento
{
    /// <summary>
    /// Fotografía antes de la reparación
    /// </summary>
    FotoAntes,

    /// <summary>
    /// Fotografía durante la reparación
    /// </summary>
    FotoDurante,

    /// <summary>
    /// Fotografía después de la reparación
    /// </summary>
    FotoDespues,

    /// <summary>
    /// Fotografía de evaluación
    /// </summary>
    FotoEvaluacion,

    /// <summary>
    /// Factura en formato PDF
    /// </summary>
    Factura,

    /// <summary>
    /// Cotización en PDF o Excel
    /// </summary>
    Cotizacion,

    /// <summary>
    /// Recibo o comprobante
    /// </summary>
    Recibo,

    /// <summary>
    /// Otro tipo de documento
    /// </summary>
    Otro
}
