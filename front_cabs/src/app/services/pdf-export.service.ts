import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { CotizacionResponse } from '../core/models/cotizacion.interface';

@Injectable({
  providedIn: 'root'
})
export class PdfExportService {

  constructor() { }

  /**
   * Exporta una cotización a PDF con formato profesional
   */
  exportarCotizacionPDF(cotizacion: CotizacionResponse): void {
    // Crear documento PDF (A4)
    const doc = new jsPDF('p', 'mm', 'a4');
    const pageWidth = doc.internal.pageSize.getWidth();
    let yPos = 20;

    // ========== ENCABEZADO ==========
    doc.setFontSize(22);
    doc.setTextColor(52, 66, 143); // #34428F
    doc.setFont('helvetica', 'bold');
    doc.text('COTIZACIÓN', pageWidth / 2, yPos, { align: 'center' });
    
    yPos += 10;
    doc.setFontSize(10);
    doc.setTextColor(107, 114, 128); // #6b7280
    doc.setFont('helvetica', 'normal');
    doc.text(`Folio: ${cotizacion.folio}`, pageWidth / 2, yPos, { align: 'center' });

    // Línea separadora
    yPos += 5;
    doc.setDrawColor(229, 231, 235); // #e5e7eb
    doc.setLineWidth(0.5);
    doc.line(15, yPos, pageWidth - 15, yPos);
    yPos += 10;

    // ========== INFORMACIÓN GENERAL ==========
    doc.setFontSize(14);
    doc.setTextColor(52, 66, 143);
    doc.setFont('helvetica', 'bold');
    doc.text('Información General', 15, yPos);
    yPos += 7;

    const infoGeneral = [
      ['ID', `#${cotizacion.id}`],
      ['Estado', this.obtenerLabelEstado(cotizacion.estado)],
      ['Validez', cotizacion.validezDias ? `${cotizacion.validezDias} días` : '-'],
      ['Fecha de Creación', new Date(cotizacion.creadoEn).toLocaleDateString('es-MX', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      })]
    ];

    autoTable(doc, {
      startY: yPos,
      head: [],
      body: infoGeneral,
      theme: 'plain',
      styles: { fontSize: 10, cellPadding: 2 },
      columnStyles: {
        0: { fontStyle: 'bold', textColor: [107, 114, 128], cellWidth: 40 },
        1: { textColor: [17, 24, 39] }
      }
    });

    yPos = (doc as any).lastAutoTable.finalY + 10;

    // ========== INFORMACIÓN DEL CLIENTE ==========
    doc.setFontSize(14);
    doc.setTextColor(52, 66, 143);
    doc.setFont('helvetica', 'bold');
    doc.text('Información del Cliente', 15, yPos);
    yPos += 7;

    const infoCliente = [
      ['Cliente', cotizacion.cliente],
      ['RFC', cotizacion.rfc],
      ['Teléfono', cotizacion.telefono || '-'],
      ['Correo', cotizacion.correo || '-']
    ];

    autoTable(doc, {
      startY: yPos,
      head: [],
      body: infoCliente,
      theme: 'plain',
      styles: { fontSize: 10, cellPadding: 2 },
      columnStyles: {
        0: { fontStyle: 'bold', textColor: [107, 114, 128], cellWidth: 40 },
        1: { textColor: [17, 24, 39] }
      }
    });

    yPos = (doc as any).lastAutoTable.finalY + 10;

    // ========== DESCRIPCIÓN DEL SERVICIO ==========
    doc.setFontSize(14);
    doc.setTextColor(52, 66, 143);
    doc.setFont('helvetica', 'bold');
    doc.text('Descripción del Servicio', 15, yPos);
    yPos += 7;

    doc.setFontSize(10);
    doc.setTextColor(17, 24, 39);
    doc.setFont('helvetica', 'normal');
    const descripcionLines = doc.splitTextToSize(cotizacion.descripcionServicio, pageWidth - 30);
    doc.text(descripcionLines, 15, yPos);
    yPos += descripcionLines.length * 5 + 5;

    // ========== OBSERVACIONES (si existen) ==========
    if (cotizacion.observaciones) {
      doc.setFontSize(14);
      doc.setTextColor(52, 66, 143);
      doc.setFont('helvetica', 'bold');
      doc.text('Observaciones', 15, yPos);
      yPos += 7;

      doc.setFontSize(10);
      doc.setTextColor(17, 24, 39);
      doc.setFont('helvetica', 'normal');
      const observacionesLines = doc.splitTextToSize(cotizacion.observaciones, pageWidth - 30);
      doc.text(observacionesLines, 15, yPos);
      yPos += observacionesLines.length * 5 + 5;
    }

    // ========== CAPACITACIÓN (si existe) ==========
    if (cotizacion.horasCapacitacion || cotizacion.paquetesCapacitacion || cotizacion.costoCapacitacion) {
      doc.setFontSize(14);
      doc.setTextColor(52, 66, 143);
      doc.setFont('helvetica', 'bold');
      doc.text('Servicios de Capacitación', 15, yPos);
      yPos += 7;

      const capacitacion = [
        ['Horas', `${cotizacion.horasCapacitacion || 0}`],
        ['Paquetes', `${cotizacion.paquetesCapacitacion || 0}`],
        ['Costo', this.formatearMoneda(cotizacion.costoCapacitacion || 0)]
      ];

      autoTable(doc, {
        startY: yPos,
        head: [],
        body: capacitacion,
        theme: 'plain',
        styles: { fontSize: 10, cellPadding: 2 },
        columnStyles: {
          0: { fontStyle: 'bold', textColor: [107, 114, 128], cellWidth: 40 },
          1: { textColor: [17, 24, 39] }
        }
      });

      yPos = (doc as any).lastAutoTable.finalY + 10;
    }

    // ========== TOTALES ==========
    doc.setFontSize(14);
    doc.setTextColor(52, 66, 143);
    doc.setFont('helvetica', 'bold');
    doc.text('Totales', 15, yPos);
    yPos += 7;

    const totalesData = [
      ['Subtotal', this.formatearMoneda(cotizacion.subtotal)],
      ['IVA (16%)', this.formatearMoneda(cotizacion.impuestosTotal)]
    ];

    if (cotizacion.descuento && cotizacion.descuento > 0) {
      totalesData.push(['Descuento', `-${this.formatearMoneda(cotizacion.descuento)}`]);
    }

    autoTable(doc, {
      startY: yPos,
      head: [],
      body: totalesData,
      theme: 'plain',
      styles: { fontSize: 10, cellPadding: 2 },
      columnStyles: {
        0: { fontStyle: 'bold', textColor: [107, 114, 128], cellWidth: 40 },
        1: { textColor: [17, 24, 39] }
      }
    });

    yPos = (doc as any).lastAutoTable.finalY + 3;

    // Total Final destacado
    doc.setDrawColor(229, 231, 235);
    doc.line(15, yPos, pageWidth - 15, yPos);
    yPos += 7;

    doc.setFontSize(14);
    doc.setTextColor(52, 66, 143);
    doc.setFont('helvetica', 'bold');
    doc.text('Total Final', 15, yPos);
    doc.text(this.formatearMoneda(cotizacion.total), pageWidth - 15, yPos, { align: 'right' });

    // ========== PIE DE PÁGINA ==========
    const footerY = doc.internal.pageSize.getHeight() - 20;
    doc.setFontSize(8);
    doc.setTextColor(107, 114, 128);
    doc.setFont('helvetica', 'italic');
    doc.text(
      `Documento generado el ${new Date().toLocaleString('es-MX')}`,
      pageWidth / 2,
      footerY,
      { align: 'center' }
    );

    // Guardar PDF
    const nombreArchivo = `Cotizacion_${cotizacion.folio}_${new Date().getTime()}.pdf`;
    doc.save(nombreArchivo);
  }

  private formatearMoneda(valor: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN'
    }).format(valor);
  }

  private obtenerLabelEstado(estado: string): string {
    const estados: { [key: string]: string } = {
      'pendiente': 'Pendiente',
      'aprobada': 'Aprobada',
      'rechazada': 'Rechazada',
      'expirada': 'Expirada'
    };
    return estados[estado.toLowerCase()] || estado;
  }
}
