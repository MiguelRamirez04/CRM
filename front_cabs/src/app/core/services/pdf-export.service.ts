import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import html2pdf from 'html2pdf.js';
import { CotizacionResponse } from '../models/cotizacion.interface';

@Injectable({
  providedIn: 'root'
})
export class PdfExportService {
  private http = inject(HttpClient);

  constructor() { }

  /**
   * Exporta una cotización a PDF usando una plantilla HTML.
   * @param cotizacion - El objeto de la cotización con todos los datos.
   */
  async exportarCotizacionPDF(cotizacion: CotizacionResponse): Promise<void> {
    try {
      // 1. Cargar la plantilla HTML desde assets
      const templateHtml = await firstValueFrom(
        this.http.get('modules/dashboard/pages/cotizaciones/cotizacion_template.html', { responseType: 'text' })
      );

      // 2. Reemplazar los placeholders con los datos de la cotización
      const finalHtml = this.rellenarPlantilla(templateHtml, cotizacion);

      // 3. Configurar opciones para html2pdf
      const options = {
        margin:       [0.5, 0.5, 0.5, 0.5] as [number, number, number, number], // top, left, bottom, right in inches
        filename:     `Cotizacion-${cotizacion.folio}.pdf`,
        html2canvas:  { scale: 2, useCORS: true }, // Aumenta la escala para mejor calidad
        jsPDF:        { unit: 'in', format: 'letter' }
      };

      // 4. Generar y descargar el PDF
      html2pdf().from(finalHtml).set(options).save();

    } catch (error) {
      console.error('Error al generar el PDF desde la plantilla HTML:', error);
      // Opcional: Mostrar una notificación de error al usuario
    }
  }

  /**
   * Rellena la plantilla HTML con los datos de la cotización.
   * @param template - El string de la plantilla HTML.
   * @param data - El objeto CotizacionResponse.
   * @returns El HTML con los datos insertados.
   */
  private rellenarPlantilla(template: string, data: CotizacionResponse): string {
    // Helper para formatear moneda
    const formatCurrency = (value: number | undefined | null) => {
      return (value ?? 0).toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });
    };

    // Helper para formatear fecha
    const formatDate = (date: string | Date) => {
      return new Date(date).toLocaleDateString('es-MX', { day: '2-digit', month: '2-digit', year: 'numeric' });
    };

    // Generar filas de la tabla de detalles (esto es un ejemplo, ajústalo a tu `CotizacionResponse`)
    // NOTA: Tu `CotizacionResponse` no tiene un array de detalles. Usaré la descripción general.
    // Si tuvieras detalles, el código sería como el que está comentado.
    let detallesHtml = `
      <tr>
        <td class="text-right">1.00</td>
        <td>Servicio</td>
        <td>${data.descripcionServicio || 'Servicio General'}</td>
        <td class="text-right">${formatCurrency(data.subtotal)}</td>
        <td class="text-right">0.00</td>
        <td class="text-right">${formatCurrency(0)}</td>
        <td class="text-right">${formatCurrency(data.subtotal)}</td>
      </tr>
    `;
    /*
    // SI TUVIERAS UN ARRAY `data.detalles`:
    let detallesHtml = data.detalles.map(item => `
      <tr>
        <td class="text-right">${item.cantidad.toFixed(2)}</td>
        <td>${item.unidad}</td>
        <td>${item.descripcion}</td>
        <td class="text-right">${formatCurrency(item.precioUnitario)}</td>
        <td class="text-right">0.00</td>
        <td class="text-right">${formatCurrency(0)}</td>
        <td class="text-right">${formatCurrency(item.importe)}</td>
      </tr>
    `).join('');
    */

    // Reemplazar todos los placeholders
    return template
      .replace(/{{folio}}/g, data.folio)
      .replace(/{{fecha}}/g, formatDate(data.creadoEn))
      .replace(/{{clienteNombre}}/g, data.cliente)
      .replace(/{{clienteRfc}}/g, data.rfc)
      .replace(/{{clienteTelefono}}/g, data.telefono?.toString() || '-')
      .replace(/{{detalles}}/g, detallesHtml)
      .replace(/{{subtotalNeto}}/g, formatCurrency(data.subtotal))
      .replace(/{{iva}}/g, formatCurrency(data.impuestosTotal))
      .replace(/{{descuentos}}/g, formatCurrency(data.descuento))
      .replace(/{{total}}/g, formatCurrency(data.totalFinal))
      // ... añade aquí el resto de reemplazos que necesites (vigencia, domicilio, etc.)
      .replace(/{{serie}}/g, 'COT') // Placeholder
      .replace(/{{vigencia}}/g, 'N/A') // Placeholder
      .replace(/{{importeConLetra}}/g, 'IMPORTE CON LETRA PENDIENTE') // Placeholder
      .replace(/{{condiciones}}/g, `<li>Precios sujetos a cambio sin previo aviso.</li><li>Pago de contado.</li>`); // Placeholder
  }
}