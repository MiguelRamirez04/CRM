using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

// Asegúrate de tener los modelos que definimos antes
// using back_cabs.CRM.models.Sales; 

namespace back_cabs.CRM.services.Recepcion
{
    public class CotizacionPdfService
    {
        private readonly ILogger<CotizacionPdfService> _logger;

        public CotizacionPdfService(ILogger<CotizacionPdfService> logger)
        {
            _logger = logger;
            // Configuración global de QuestPDF (solo se hace una vez)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Ya no necesita ser 'async' porque no leemos plantillas
        public byte[] GenerarCotizacionPdf(CotizacionModel cotizacion)
        {
            try
            {
                _logger.LogInformation("🔄 Generando PDF para cotización {Folio}", cotizacion.Folio);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Configuración de la página
                        page.Margin(30);
                        page.DefaultTextStyle(TextStyle.Default.FontSize(10));

                        // 1. Encabezado
                        page.Header().Element(BuildHeader);

                        // 2. Contenido (Datos del cliente y tabla)
                        page.Content().Element(content => BuildContent(content, cotizacion));

                        // 3. Pie de página
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                        });
                    });
                });

                // Genera el PDF en memoria
                byte[] pdfBytes = document.GeneratePdf(); 
                _logger.LogInformation("✅ PDF generado: {Folio}", cotizacion.Folio);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error PDF cotización {Folio}", cotizacion.Folio);
                throw;
            }
        }

        // --- Métodos de Ayuda para construir el PDF ---

        // Construye el Encabezado (Logo, Folio, etc.)
        private void BuildHeader(IContainer container)
        {
            container.Row(row =>
            {
                // Columna para el Logo (si tuvieras)
                row.ConstantItem(100).Text("CABS COMPUTACIÓN")
                   .Bold().FontSize(16);

                row.RelativeItem().Column(col =>
                {
                    col.Item().AlignRight().Text("Cotización")
                       .Bold().FontSize(14);
                    // Aquí podrías añadir Folio, Fecha, etc.
                    // col.Item().AlignRight().Text($"Folio: {cotizacion.Folio}");
                    // col.Item().AlignRight().Text($"Fecha: {cotizacion.Fecha.ToShortDateString()}");
                });
            });
        }

        // Construye el Contenido (Cliente, Tabla, Totales)
        private void BuildContent(IContainer container, CotizacionModel cotizacion)
        {
            container.PaddingVertical(20).Column(col =>
            {
                // --- 2.1 Datos del Cliente ---
                col.Item().Element(box => BuildClientInfo(box, cotizacion));
                col.Item().PaddingVertical(10); // Espacio

                // --- 2.2 Tabla de Items ---
                col.Item().Element(box => BuildItemsTable(box, cotizacion));
                col.Item().PaddingVertical(10); // Espacio

                // --- 2.3 Totales ---
                col.Item().AlignRight().Element(box => BuildTotals(box, cotizacion));
                
                // --- 2.4 Condiciones ---
                col.Item().PaddingTop(25).Element(box => BuildConditions(box, cotizacion));
            });
        }

        private void BuildClientInfo(IContainer container, CotizacionModel cotizacion)
        {
            container.Border(1).Padding(10).Column(col =>
            {
                col.Item().Text("Datos del Cliente").Bold();
                col.Item().Text($"Cliente: {cotizacion.ClienteNombre}");
                col.Item().Text($"RFC: {cotizacion.ClienteRfc}");
                col.Item().Text($"Domicilio: {cotizacion.ClienteDomicilio}");
            });
        }

        private void BuildItemsTable(IContainer container, CotizacionModel cotizacion)
        {
            container.Table(table =>
            {
                // Definición de columnas
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50); // Cantidad
                    columns.RelativeColumn(3);  // Descripción
                    columns.RelativeColumn();   // P. Unitario
                    columns.RelativeColumn();   // Importe
                });

                // Encabezado de la tabla
                table.Header(header =>
                {
                    header.Cell().Background("#EEE").Padding(5).Text("Cant.");
                    header.Cell().Background("#EEE").Padding(5).Text("Descripción");
                    header.Cell().Background("#EEE").Padding(5).Text("P. Unitario");
                    header.Cell().Background("#EEE").Padding(5).Text("Importe");
                });

                // --- Filas de la tabla (los productos) ---
                foreach (var item in cotizacion.Detalles)
                {
                    table.Cell().BorderBottom(1).BorderColor("#CCC").Padding(5)
                         .Text(item.Cantidad.ToString());
                    
                    table.Cell().BorderBottom(1S).BorderColor("#CCC").Padding(5)
                         .Text(item.Descripcion);
                    
                    table.Cell().BorderBottom(1).BorderColor("#CCC").Padding(5)
                         .Text($"${item.PrecioUnitario:N2}");
                    
                    table.Cell().BorderBottom(1).BorderColor("#CCC").Padding(5)
                         .Text($"${item.Importe:N2}");
                }
            });
        }

        private void BuildTotals(IContainer container, CotizacionModel cotizacion)
        {
            // Un contenedor de 200px de ancho para alinear los totales
            container.Width(200).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:");
                    row.ConstantItem(80).AlignRight().Text($"${cotizacion.SubtotalNeto:N2}");
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("IVA (16%):");
                    row.ConstantItem(80).AlignRight().Text($"${cotizacion.Iva:N2}");
                });
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Bold().Text("Total:");
                    row.ConstantItem(80).AlignRight().Bold().Text($"${cotizacion.Total:N2}");
                });
            });
        }
        
        private void BuildConditions(IContainer container, CotizacionModel cotizacion)
        {
            container.Column(col =>
            {
                col.Item().Text("Importe con letra:").Bold();
                col.Item().Text(cotizacion.ImporteConLetra);

                col.Item().PaddingTop(10).Text("Condiciones de Venta:").Bold();
                foreach (var condicion in cotizacion.Condiciones)
                {
                    col.Item().Text(condicion);
                }
            });
        }
    }
}