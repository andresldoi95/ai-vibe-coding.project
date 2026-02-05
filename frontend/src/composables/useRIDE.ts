/**
 * RIDE (Representación Impresa del Documento Electrónico) Generator
 * Generates PDF invoices for Ecuador SRI compliance
 */
import jsPDF from 'jspdf'
import autoTable from 'jspdf-autotable'
import { useCurrency } from './useCurrency'

export interface ICompanyInfo {
  businessName: string
  tradeName?: string
  ruc: string
  address: string
  phone?: string
  email?: string
  accountingRequired: boolean
  specialTaxpayerNumber?: string
}

export interface IInvoiceData {
  accessKey: string
  documentNumber: string
  authorizationNumber?: string
  authorizationDate?: Date
  issueDate: Date
  customer: {
    name: string
    identification: string
    address?: string
    phone?: string
    email?: string
  }
  items: Array<{
    code: string
    description: string
    quantity: number
    price: number
    discount: number
    subtotal: number
  }>
  subtotal: number
  subtotalIva12: number
  subtotalIva0: number
  subtotalNoIva: number
  discount: number
  iva: number
  total: number
  paymentMethod: string
}

export function useRIDE() {
  const { formatCurrency, numberToWords } = useCurrency()

  /**
   * Generate RIDE PDF for invoice
   */
  const generateInvoiceRIDE = (
    company: ICompanyInfo,
    invoice: IInvoiceData
  ): jsPDF => {
    const doc = new jsPDF()
    const pageWidth = doc.internal.pageSize.width
    let yPos = 20

    // Header - Company info
    doc.setFontSize(16)
    doc.setFont('helvetica', 'bold')
    doc.text(company.tradeName || company.businessName, pageWidth / 2, yPos, { align: 'center' })
    
    yPos += 7
    doc.setFontSize(10)
    doc.setFont('helvetica', 'normal')
    doc.text(company.businessName, pageWidth / 2, yPos, { align: 'center' })
    
    yPos += 5
    doc.text(`RUC: ${company.ruc}`, pageWidth / 2, yPos, { align: 'center' })
    
    yPos += 5
    doc.text(company.address, pageWidth / 2, yPos, { align: 'center' })
    
    if (company.phone || company.email) {
      yPos += 5
      const contact = [company.phone, company.email].filter(Boolean).join(' - ')
      doc.text(contact, pageWidth / 2, yPos, { align: 'center' })
    }
    
    if (company.accountingRequired) {
      yPos += 5
      doc.text('OBLIGADO A LLEVAR CONTABILIDAD', pageWidth / 2, yPos, { align: 'center' })
    }

    // Invoice type and number
    yPos += 10
    doc.setFontSize(14)
    doc.setFont('helvetica', 'bold')
    doc.text('FACTURA', pageWidth / 2, yPos, { align: 'center' })
    
    yPos += 7
    doc.setFontSize(12)
    doc.text(`No. ${invoice.documentNumber}`, pageWidth / 2, yPos, { align: 'center' })

    // Access key
    yPos += 7
    doc.setFontSize(8)
    doc.setFont('helvetica', 'normal')
    doc.text('CLAVE DE ACCESO', 14, yPos)
    yPos += 4
    doc.text(invoice.accessKey, 14, yPos)

    // Authorization info (if authorized)
    if (invoice.authorizationNumber && invoice.authorizationDate) {
      yPos += 5
      doc.text(`AUTORIZACIÓN SRI: ${invoice.authorizationNumber}`, 14, yPos)
      yPos += 4
      doc.text(`FECHA AUTORIZACIÓN: ${invoice.authorizationDate.toLocaleDateString('es-EC')}`, 14, yPos)
    } else {
      yPos += 5
      doc.text('PENDIENTE DE AUTORIZACIÓN', 14, yPos)
    }

    // Issue date
    yPos += 5
    doc.text(`FECHA EMISIÓN: ${invoice.issueDate.toLocaleDateString('es-EC')}`, 14, yPos)

    // Customer info
    yPos += 10
    doc.setFontSize(10)
    doc.setFont('helvetica', 'bold')
    doc.text('DATOS DEL CLIENTE', 14, yPos)
    
    yPos += 6
    doc.setFont('helvetica', 'normal')
    doc.text(`Razón Social: ${invoice.customer.name}`, 14, yPos)
    
    yPos += 5
    doc.text(`RUC/Cédula: ${invoice.customer.identification}`, 14, yPos)
    
    if (invoice.customer.address) {
      yPos += 5
      doc.text(`Dirección: ${invoice.customer.address}`, 14, yPos)
    }
    
    if (invoice.customer.phone || invoice.customer.email) {
      yPos += 5
      const contact = [invoice.customer.phone, invoice.customer.email].filter(Boolean).join(' - ')
      doc.text(`Contacto: ${contact}`, 14, yPos)
    }

    // Items table
    yPos += 10
    autoTable(doc, {
      startY: yPos,
      head: [['Código', 'Descripción', 'Cant.', 'P. Unit.', 'Desc.', 'Subtotal']],
      body: invoice.items.map(item => [
        item.code,
        item.description,
        item.quantity.toString(),
        formatCurrency(item.price),
        formatCurrency(item.discount),
        formatCurrency(item.subtotal)
      ]),
      theme: 'grid',
      headStyles: { fillColor: [66, 66, 66], fontSize: 9 },
      bodyStyles: { fontSize: 9 },
      columnStyles: {
        0: { cellWidth: 25 },
        1: { cellWidth: 70 },
        2: { cellWidth: 20, halign: 'right' },
        3: { cellWidth: 25, halign: 'right' },
        4: { cellWidth: 25, halign: 'right' },
        5: { cellWidth: 25, halign: 'right' }
      }
    })

    // Get Y position after table
    yPos = (doc as any).lastAutoTable.finalY + 10

    // Totals
    const totalsX = pageWidth - 70
    doc.setFontSize(9)
    
    if (invoice.subtotalIva12 > 0) {
      doc.text('Subtotal IVA 12%:', totalsX, yPos)
      doc.text(formatCurrency(invoice.subtotalIva12), pageWidth - 14, yPos, { align: 'right' })
      yPos += 5
    }
    
    if (invoice.subtotalIva0 > 0) {
      doc.text('Subtotal IVA 0%:', totalsX, yPos)
      doc.text(formatCurrency(invoice.subtotalIva0), pageWidth - 14, yPos, { align: 'right' })
      yPos += 5
    }
    
    if (invoice.subtotalNoIva > 0) {
      doc.text('Subtotal No IVA:', totalsX, yPos)
      doc.text(formatCurrency(invoice.subtotalNoIva), pageWidth - 14, yPos, { align: 'right' })
      yPos += 5
    }
    
    if (invoice.discount > 0) {
      doc.text('Descuento:', totalsX, yPos)
      doc.text(formatCurrency(invoice.discount), pageWidth - 14, yPos, { align: 'right' })
      yPos += 5
    }
    
    doc.text('Subtotal:', totalsX, yPos)
    doc.text(formatCurrency(invoice.subtotal), pageWidth - 14, yPos, { align: 'right' })
    yPos += 5
    
    doc.text('IVA 12%:', totalsX, yPos)
    doc.text(formatCurrency(invoice.iva), pageWidth - 14, yPos, { align: 'right' })
    yPos += 7
    
    doc.setFont('helvetica', 'bold')
    doc.setFontSize(11)
    doc.text('TOTAL:', totalsX, yPos)
    doc.text(formatCurrency(invoice.total), pageWidth - 14, yPos, { align: 'right' })

    // Total in words
    yPos += 8
    doc.setFontSize(8)
    doc.setFont('helvetica', 'normal')
    doc.text(`SON: ${numberToWords(invoice.total)}`, 14, yPos)

    // Payment method
    yPos += 6
    doc.text(`FORMA DE PAGO: ${invoice.paymentMethod}`, 14, yPos)

    // Footer
    const footerY = doc.internal.pageSize.height - 20
    doc.setFontSize(7)
    doc.text('Este documento es una representación gráfica de un comprobante electrónico', pageWidth / 2, footerY, { align: 'center' })
    doc.text('autorizado por el Servicio de Rentas Internas (SRI)', pageWidth / 2, footerY + 4, { align: 'center' })

    return doc
  }

  /**
   * Download RIDE PDF
   */
  const downloadRIDE = (
    company: ICompanyInfo,
    invoice: IInvoiceData,
    filename?: string
  ): void => {
    const doc = generateInvoiceRIDE(company, invoice)
    const fn = filename || `FACTURA_${invoice.documentNumber.replace(/-/g, '_')}.pdf`
    doc.save(fn)
  }

  /**
   * Print RIDE PDF
   */
  const printRIDE = (
    company: ICompanyInfo,
    invoice: IInvoiceData
  ): void => {
    const doc = generateInvoiceRIDE(company, invoice)
    doc.autoPrint()
    window.open(doc.output('bloburl'), '_blank')
  }

  return {
    generateInvoiceRIDE,
    downloadRIDE,
    printRIDE
  }
}
