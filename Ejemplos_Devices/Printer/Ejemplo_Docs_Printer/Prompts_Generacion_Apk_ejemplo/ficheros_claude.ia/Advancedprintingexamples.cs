using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermalPrinterMAUI.Services;

namespace ThermalPrinterMAUI.Examples
{
    /// <summary>
    /// Ejemplos avanzados de uso del servicio de impresión térmica
    /// </summary>
    public class AdvancedPrintingExamples
    {
        private readonly IThermalPrinterService _printer;

        public AdvancedPrintingExamples(IThermalPrinterService printer)
        {
            _printer = printer;
        }

        /// <summary>
        /// Imprime un ticket de restaurante
        /// </summary>
        public async Task PrintRestaurantTicket()
        {
            await _printer.PrintTextAsync("*** RESTAURANTE LA BUENA MESA ***", fontSize: 1, bold: true, centered: true);
            await _printer.PrintTextAsync("Calle Principal 123", centered: true);
            await _printer.PrintTextAsync("Tel: (123) 456-7890", centered: true);
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync($"Mesa: 5        Mesero: Juan");
            await _printer.PrintTextAsync($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            await _printer.PrintTextAsync("================================");

            // Items del pedido
            var items = new List<(string name, int qty, decimal price)>
            {
                ("Entrada César", 2, 85.00m),
                ("Lomo saltado", 2, 120.00m),
                ("Bebida", 2, 25.00m),
                ("Postre del día", 1, 45.00m)
            };

            await _printer.PrintTextAsync("CANT  DESCRIPCION       TOTAL");
            await _printer.PrintTextAsync("--------------------------------");

            decimal subtotal = 0;
            foreach (var item in items)
            {
                decimal total = item.qty * item.price;
                subtotal += total;

                string line = $" {item.qty}x   {item.name,-15} ${total,6:F2}";
                await _printer.PrintTextAsync(line);
            }

            await _printer.PrintTextAsync("================================");
            await _printer.PrintTextAsync($"SUBTOTAL:                ${subtotal:F2}", bold: false);

            decimal iva = subtotal * 0.16m;
            await _printer.PrintTextAsync($"IVA (16%):               ${iva:F2}");

            decimal total_final = subtotal + iva;
            await _printer.PrintTextAsync($"TOTAL:                   ${total_final:F2}", fontSize: 2, bold: true);

            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync("¡Gracias por su visita!", centered: true, bold: true);
            await _printer.PrintTextAsync("Vuelva pronto", centered: true);

            await _printer.FeedLinesAsync(4);
            await _printer.CutPaperAsync();
        }

        /// <summary>
        /// Imprime un comprobante de pago
        /// </summary>
        public async Task PrintPaymentReceipt(string cardNumber, decimal amount, string authCode)
        {
            await _printer.PrintTextAsync("COMPROBANTE DE PAGO", fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync("MI COMERCIO S.A.", bold: true, centered: true);
            await _printer.PrintTextAsync("RFC: ABC123456789", centered: true);
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync($"Fecha: {DateTime.Now:dd/MM/yyyy}");
            await _printer.PrintTextAsync($"Hora: {DateTime.Now:HH:mm:ss}");
            await _printer.PrintTextAsync($"Terminal: 001");
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync("VENTA", bold: true);
            await _printer.PrintTextAsync($"Tarjeta: ****{cardNumber.Substring(cardNumber.Length - 4)}");
            await _printer.PrintTextAsync($"Autorizacion: {authCode}");
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync($"TOTAL: ${amount:F2}", fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync("APROBADA", fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync("Firma: __________________", centered: true);

            await _printer.FeedLinesAsync(4);
            await _printer.CutPaperAsync();
        }

        /// <summary>
        /// Imprime una factura simplificada
        /// </summary>
        public async Task PrintInvoice(string invoiceNumber, string customerName)
        {
            await _printer.PrintTextAsync("FACTURA SIMPLIFICADA", fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync($"Factura No: {invoiceNumber}", bold: true);
            await _printer.PrintTextAsync($"Fecha: {DateTime.Now:dd/MM/yyyy}");
            await _printer.PrintTextAsync($"Cliente: {customerName}");
            await _printer.PrintTextAsync("================================");

            var items = new List<(string description, int qty, decimal unitPrice)>
            {
                ("Producto A", 5, 100.00m),
                ("Producto B", 2, 250.00m),
                ("Servicio C", 1, 500.00m)
            };

            await _printer.PrintTextAsync("DESCRIPCION      CANT  P.UNIT  TOTAL");
            await _printer.PrintTextAsync("--------------------------------");

            decimal subtotal = 0;
            foreach (var item in items)
            {
                decimal lineTotal = item.qty * item.unitPrice;
                subtotal += lineTotal;

                await _printer.PrintTextAsync($"{item.description,-12} {item.qty,4} {item.unitPrice,6:F2} {lineTotal,7:F2}");
            }

            await _printer.PrintTextAsync("================================");
            await _printer.PrintTextAsync($"Subtotal:                ${subtotal:F2}");

            decimal tax = subtotal * 0.16m;
            await _printer.PrintTextAsync($"IVA (16%):               ${tax:F2}");

            decimal total = subtotal + tax;
            await _printer.PrintTextAsync($"TOTAL:                   ${total:F2}", fontSize: 1, bold: true);

            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync("Sello Digital:", bold: true);
            await _printer.PrintQRCodeAsync($"FACTURA:{invoiceNumber}|TOTAL:{total}|FECHA:{DateTime.Now:yyyyMMdd}", size: 6);

            await _printer.FeedLinesAsync(4);
            await _printer.CutPaperAsync();
        }

        /// <summary>
        /// Imprime una etiqueta de producto
        /// </summary>
        public async Task PrintProductLabel(string productName, string sku, decimal price)
        {
            await _printer.PrintTextAsync(productName, fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync($"SKU: {sku}", centered: true);
            await _printer.PrintTextAsync("");

            // Código de barras
            await _printer.PrintBarcodeAsync(sku, BarcodeType.CODE128);

            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync($"${price:F2}", fontSize: 3, bold: true, centered: true);

            await _printer.FeedLinesAsync(2);
            await _printer.CutPaperAsync();
        }

        /// <summary>
        /// Imprime un ticket de evento
        /// </summary>
        public async Task PrintEventTicket(string eventName, string venue, DateTime eventDate, string seat, string ticketId)
        {
            await _printer.PrintTextAsync("*** ENTRADA ***", fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync(eventName, fontSize: 1, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync($"Lugar: {venue}");
            await _printer.PrintTextAsync($"Fecha: {eventDate:dd/MM/yyyy}");
            await _printer.PrintTextAsync($"Hora: {eventDate:HH:mm}");
            await _printer.PrintTextAsync($"Asiento: {seat}", bold: true);
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync("Código de verificación:", centered: true);
            await _printer.PrintQRCodeAsync(ticketId, size: 8);

            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync($"ID: {ticketId}", fontSize: 1, centered: true);
            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync("Conserve este ticket", centered: true);

            await _printer.FeedLinesAsync(4);
            await _printer.CutPaperAsync();
        }

        /// <summary>
        /// Imprime un turno de atención
        /// </summary>
        public async Task PrintQueueTicket(string department, int queueNumber)
        {
            await _printer.PrintTextAsync("SU TURNO", fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync(department, fontSize: 1, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync(queueNumber.ToString(), fontSize: 4, bold: true, centered: true);

            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync($"Fecha: {DateTime.Now:dd/MM/yyyy}", centered: true);
            await _printer.PrintTextAsync($"Hora: {DateTime.Now:HH:mm}", centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync("Por favor espere", centered: true);
            await _printer.PrintTextAsync("a ser llamado", centered: true);

            await _printer.FeedLinesAsync(4);
            await _printer.CutPaperAsync();
        }

        /// <summary>
        /// Imprime un recibo de donación
        /// </summary>
        public async Task PrintDonationReceipt(string donorName, decimal amount, string cause)
        {
            await _printer.PrintTextAsync("RECIBO DE DONACION", fontSize: 1, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync("FUNDACION SOLIDARIA A.C.", bold: true, centered: true);
            await _printer.PrintTextAsync("RFC: FSO123456ABC", centered: true);
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync($"Donante: {donorName}");
            await _printer.PrintTextAsync($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            await _printer.PrintTextAsync($"Causa: {cause}");
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync("MONTO DONADO", bold: true, centered: true);
            await _printer.PrintTextAsync($"${amount:F2}", fontSize: 3, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync("¡Gracias por su generosidad!", centered: true, bold: true);
            await _printer.PrintTextAsync("Su donacion es deducible", centered: true);
            await _printer.PrintTextAsync("de impuestos", centered: true);

            await _printer.FeedLinesAsync(4);
            await _printer.CutPaperAsync();
        }

        /// <summary>
        /// Imprime un reporte de cierre de caja
        /// </summary>
        public async Task PrintCashClosingReport(DateTime date, decimal initialCash, decimal sales, decimal expenses, decimal finalCash)
        {
            await _printer.PrintTextAsync("CIERRE DE CAJA", fontSize: 2, bold: true, centered: true);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync($"Fecha: {date:dd/MM/yyyy}", centered: true);
            await _printer.PrintTextAsync($"Hora: {DateTime.Now:HH:mm:ss}", centered: true);
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync("EFECTIVO INICIAL:", bold: true);
            await _printer.PrintTextAsync($"${initialCash:F2}", fontSize: 2);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync("VENTAS DEL DIA:", bold: true);
            await _printer.PrintTextAsync($"${sales:F2}", fontSize: 2);
            await _printer.PrintTextAsync("");

            await _printer.PrintTextAsync("GASTOS:", bold: true);
            await _printer.PrintTextAsync($"-${expenses:F2}", fontSize: 2);
            await _printer.PrintTextAsync("================================");

            await _printer.PrintTextAsync("EFECTIVO FINAL:", fontSize: 1, bold: true);
            await _printer.PrintTextAsync($"${finalCash:F2}", fontSize: 3, bold: true);
            await _printer.PrintTextAsync("");

            decimal expected = initialCash + sales - expenses;
            decimal difference = finalCash - expected;

            await _printer.PrintTextAsync($"Esperado: ${expected:F2}");
            await _printer.PrintTextAsync($"Diferencia: ${difference:F2}", bold: difference != 0);

            await _printer.PrintTextAsync("");
            await _printer.PrintTextAsync("Firma: _______________", centered: true);

            await _printer.FeedLinesAsync(4);
            await _printer.CutPaperAsync();
        }
    }
}