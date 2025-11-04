import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import autoTable from 'jspdf-autotable';
import { CotizacionResponse } from '../models/cotizacion.interface';

@Injectable({ providedIn: 'root' })
export class PdfExportService {
  constructor() {}

  // Helper to safely read lastAutoTable.finalY without TypeScript errors
  private getLastAutoTableFinalY(doc: jsPDF): number {
    const last = (doc as any).lastAutoTable;
    return last && typeof last.finalY === 'number' ? last.finalY : 55;
  }

  async exportCotizacion(cotizacion: CotizacionResponse): Promise<void> {
    const doc = new jsPDF();
    const pageWidth = doc.internal.pageSize.getWidth();
    
    // Configuración de estilos
    const primaryColor = '#34428F';
    doc.setDrawColor(primaryColor);
    doc.setFillColor(primaryColor);
    
    // Encabezado
    doc.setFontSize(24);
    doc.setTextColor(primaryColor);
    doc.text('COTIZACIÓN', pageWidth / 2, 20, { align: 'center' });
    
    // Info de la cotización
    doc.setFontSize(12);
    doc.setTextColor(100);
    doc.text(`Folio: ${cotizacion.folio}`, 20, 35);
    doc.text(`Fecha: ${new Date(cotizacion.creadoEn).toLocaleDateString()}`, pageWidth - 20, 35, { align: 'right' });
    
    // Línea divisoria
    doc.setLineWidth(0.5);
    doc.line(20, 40, pageWidth - 20, 40);
    
    // Información del cliente
    doc.setFontSize(14);
    doc.setTextColor(primaryColor);
    doc.text('Información del Cliente', 20, 50);
    
    doc.setFontSize(12);
    doc.setTextColor(100);
    const clienteInfo = [
      ['Cliente:', cotizacion.cliente],
      ['RFC:', cotizacion.rfc],
      ['Teléfono:', cotizacion.telefono || 'N/A'],
      ['Correo:', cotizacion.correo || 'N/A']
    ];
    
    autoTable(doc, {
      startY: 55,
      head: [],
      body: clienteInfo,
      theme: 'plain',
      styles: { fontSize: 11, cellPadding: 2 },
      columnStyles: { 
        0: { fontStyle: 'bold', cellWidth: 30 },
        1: { cellWidth: 100 }
      }
    });
    
    // Detalles del servicio
    doc.setFontSize(14);
    doc.setTextColor(primaryColor);
    doc.text('Descripción del Servicio', 20, this.getLastAutoTableFinalY(doc) + 15);
    
    doc.setFontSize(11);
    doc.setTextColor(100);
    const splitDesc = doc.splitTextToSize(cotizacion.descripcionServicio, pageWidth - 40);
    doc.text(splitDesc, 20, this.getLastAutoTableFinalY(doc) + 25);
    
    // Si hay capacitación
    let yPos = this.getLastAutoTableFinalY(doc) + 25 + (splitDesc.length * 7);
    if (cotizacion.horasCapacitacion || cotizacion.paquetesCapacitacion) {
      doc.setFontSize(14);
      doc.setTextColor(primaryColor);
      doc.text('Servicios de Capacitación', 20, yPos + 10);
      
      const capacitacionInfo = [
        ['Horas:', cotizacion.horasCapacitacion?.toString() || 'N/A'],
        ['Paquetes:', cotizacion.paquetesCapacitacion?.toString() || 'N/A'],
        ['Costo:', cotizacion.costoCapacitacion ? 
          `$${cotizacion.costoCapacitacion.toLocaleString('es-MX', { minimumFractionDigits: 2 })}` : 
          'N/A']
      ];
      
      autoTable(doc, {
        startY: yPos + 15,
        head: [],
        body: capacitacionInfo,
        theme: 'plain',
        styles: { fontSize: 11, cellPadding: 2 },
        columnStyles: {
          0: { fontStyle: 'bold', cellWidth: 30 },
          1: { cellWidth: 100 }
        }
      });
      
      yPos = this.getLastAutoTableFinalY(doc);
    }
    
    // Información financiera
    doc.setFontSize(14);
    doc.setTextColor(primaryColor);
    doc.text('Resumen Financiero', 20, yPos + 15);
    
    const financieroInfo = [
      ['Subtotal:', `$${cotizacion.subtotal.toLocaleString('es-MX', { minimumFractionDigits: 2 })}`],
      ['IVA (16%):', `$${cotizacion.impuestosTotal.toLocaleString('es-MX', { minimumFractionDigits: 2 })}`]
    ];
    
    if (cotizacion.descuento && cotizacion.descuento > 0) {
      financieroInfo.push(['Descuento:', `$${cotizacion.descuento.toLocaleString('es-MX', { minimumFractionDigits: 2 })}`]);
    }
    
    financieroInfo.push(['Total:', `$${cotizacion.total.toLocaleString('es-MX', { minimumFractionDigits: 2 })}`]);
    
    autoTable(doc, {
      startY: yPos + 20,
      head: [],
      body: financieroInfo,
      theme: 'plain',
      styles: { fontSize: 11, cellPadding: 3 },
      columnStyles: {
        0: { fontStyle: 'bold', cellWidth: 30 },
        1: { cellWidth: 100, halign: 'right' }
      }
    });
    
    // Validez y observaciones al final
    yPos = this.getLastAutoTableFinalY(doc) + 15;
    doc.setFontSize(11);
    doc.setTextColor(100);
    doc.text(`Validez: ${cotizacion.validezDias} días`, 20, yPos);
    
    if (cotizacion.observaciones) {
      doc.setFontSize(14);
      doc.setTextColor(primaryColor);
      doc.text('Observaciones', 20, yPos + 15);
      
      doc.setFontSize(11);
      doc.setTextColor(100);
      const splitObs = doc.splitTextToSize(cotizacion.observaciones, pageWidth - 40);
      doc.text(splitObs, 20, yPos + 25);
    }
    
    // Guardar el PDF
    doc.save(`Cotizacion-${cotizacion.folio}.pdf`);
  }

  async exportElementToPdf(element: HTMLElement, fileName: string): Promise<void> {
    if (!element) throw new Error('Elemento no encontrado para exportar');

    // Aumentar escala para mejorar resolución
    const canvas = await html2canvas(element, { scale: 2, useCORS: true });
    const imgData = canvas.toDataURL('image/png');

    const pdf = new jsPDF('p', 'mm', 'a4');
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();

    // Ajustar imagen al ancho útil de la página con margen
    const margin = 10;
    const imgWidth = pageWidth - margin * 2;
    const imgHeight = (canvas.height * imgWidth) / canvas.width;

    let position = margin;

    // Si la imagen es mayor que la página en alto, dividir en páginas
    if (imgHeight <= pageHeight - margin * 2) {
      pdf.addImage(imgData, 'PNG', margin, position, imgWidth, imgHeight);
    } else {
      // Añadir porciones en páginas
      let remainingHeight = canvas.height;
      let renderedHeight = 0;
      const pageCanvas = document.createElement('canvas');
      const pageCtx = pageCanvas.getContext('2d') as CanvasRenderingContext2D;

      // Ajuste de escala para que el corte coincida con la proporción
      const scale = imgWidth / canvas.width;
      const pagePixelHeight = Math.floor((pageHeight - margin * 2) / scale);

      pageCanvas.width = canvas.width;
      pageCanvas.height = pagePixelHeight;

      while (renderedHeight < canvas.height) {
        pageCtx.clearRect(0, 0, pageCanvas.width, pageCanvas.height);
        pageCtx.drawImage(canvas, 0, renderedHeight, canvas.width, pagePixelHeight, 0, 0, pageCanvas.width, pageCanvas.height);

        const pageData = pageCanvas.toDataURL('image/png');
        if (renderedHeight > 0) pdf.addPage();
        pdf.addImage(pageData, 'PNG', margin, margin, imgWidth, pagePixelHeight * scale);

        renderedHeight += pagePixelHeight;
      }
    }

    pdf.save(fileName);
  }
}
