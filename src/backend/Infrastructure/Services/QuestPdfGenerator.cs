using System.Globalization;
using Core.Application.Abstractions;
using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.ValueObjects;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services;

/// <summary>Documentos imprimibles con QuestPDF (licencia Community, uso académico).</summary>
public sealed class QuestPdfGenerator(IClock clock) : IDocumentPdfGenerator
{
    private const string CompanyName = "Distribución Polar";
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    static QuestPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] RenderInvoiceTicket(Invoice invoice, Client? client)
    {
        return Document.Create(container => container.Page(page =>
        {
            page.ContinuousSize(80, Unit.Millimetre);
            page.Margin(4, Unit.Millimetre);
            page.DefaultTextStyle(t => t.FontSize(8));

            page.Content().Column(col =>
            {
                col.Spacing(3);
                col.Item().AlignCenter().Text(CompanyName).Bold().FontSize(11);
                col.Item().AlignCenter().Text(invoice.Number).Bold();
                col.Item().AlignCenter().Text(clock.ToLocal(invoice.IssuedAt).ToString("dd/MM/yyyy HH:mm", Invariant));
                if (invoice.IsCancelled)
                {
                    col.Item().AlignCenter().Text("*** ANULADA ***").Bold().FontColor(Colors.Red.Medium);
                }

                Section(col, "CLIENTE");
                col.Item().Text($"{invoice.ClientName}");
                col.Item().Text($"RIF/CI: {invoice.ClientRif}");
                if (client is not null)
                {
                    col.Item().Text($"Dirección: {client.Address}");
                    if (client.Nickname is not null)
                    {
                        col.Item().Text($"Referencia: {client.Nickname}");
                    }
                }

                Section(col, "DESPACHO");
                col.Item().Text(invoice.DispatchOrigin == StockOrigin.Warehouse ? "Origen: Galpón" : $"Origen: Camión {invoice.TruckName} ({invoice.TruckPlate})");
                col.Item().Text($"Atendió: {invoice.AttendantName}");

                Section(col, "PRODUCTOS");
                foreach (var item in invoice.Items)
                {
                    col.Item().Text(item.ProductName);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text(item.Quantity.ToString());
                        row.ConstantItem(60).AlignRight().Text(Money(item.Subtotal));
                    });
                }

                col.Item().LineHorizontal(0.5f);
                Total(col, "TOTAL", invoice.Total, bold: true);
                if (invoice.Type == InvoiceType.Sale)
                {
                    Total(col, "Abono", invoice.Payment);
                    Total(col, invoice.Pending >= 0 ? "Pendiente" : "Saldo a favor", Math.Abs(invoice.Pending));
                }

                if (!string.IsNullOrWhiteSpace(invoice.PaymentObservation))
                {
                    Section(col, "OBSERVACIÓN DE PAGO");
                    col.Item().Text(invoice.PaymentObservation);
                }

                if (invoice.EmptyGroups.Count > 0)
                {
                    Section(col, "VACÍOS RETORNABLES");
                    foreach (var group in invoice.EmptyGroups)
                    {
                        col.Item().Text(EmptyReturnGroups.GetName(group.GroupKey)).SemiBold();
                        col.Item().Text($"Generados {group.GeneratedBoxes} cj + {group.GeneratedUnits} und · Devueltos {group.ReturnedBoxes} cj + {group.ReturnedUnits} und");
                    }
                }

                col.Item().PaddingTop(14).Text("Recibido por: ______________________");
                col.Item().PaddingTop(8).Text("Firma: ______________________________");
                col.Item().PaddingTop(8).AlignCenter().Text("Gracias por su compra").Italic();
            });
        })).GeneratePdf();
    }

    public byte[] RenderConsignment(ConsignmentEvent consignmentEvent)
    {
        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.Letter);
            page.Margin(20, Unit.Millimetre);
            page.DefaultTextStyle(t => t.FontSize(10));

            page.Header().Column(col =>
            {
                col.Item().Text(CompanyName).Bold().FontSize(16);
                col.Item().Text($"Evento a consignación: {consignmentEvent.Name}").FontSize(12);
                col.Item().Text($"Responsable: {consignmentEvent.Responsible} · Fecha: {clock.ToLocal(consignmentEvent.EventDate):dd/MM/yyyy}");
                col.Item().Text(consignmentEvent.IsClosed ? $"Estado: Cerrado ({clock.ToLocal(consignmentEvent.ClosedAt!.Value):dd/MM/yyyy})" : "Estado: Abierto");
            });

            page.Content().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                });

                table.Header(h =>
                {
                    foreach (var title in new[] { "Producto", "Entregado", "Devuelto", "Vendido", "Monto" })
                    {
                        h.Cell().BorderBottom(1).PaddingBottom(3).Text(title).Bold();
                    }
                });

                foreach (var item in consignmentEvent.Items.OrderBy(i => i.ProductName))
                {
                    table.Cell().PaddingVertical(2).Text(item.ProductName);
                    table.Cell().Text(BoxQuantity.FromUnits(item.DeliveredUnits, item.UnitsPerBox).ToString());
                    table.Cell().Text(BoxQuantity.FromUnits(item.ReturnedUnits, item.UnitsPerBox).ToString());
                    table.Cell().Text(BoxQuantity.FromUnits(item.SoldUnits, item.UnitsPerBox).ToString());
                    table.Cell().AlignRight().Text(Money(item.SoldAmount));
                }
            });

            page.Footer().AlignRight().Text($"Total a cobrar: {Money(consignmentEvent.TotalSold)}").Bold().FontSize(12);
        })).GeneratePdf();
    }

    private static void Section(ColumnDescriptor col, string title)
    {
        col.Item().PaddingTop(4).Text(title).Bold();
    }

    private static void Total(ColumnDescriptor col, string label, decimal amount, bool bold = false)
    {
        col.Item().Row(row =>
        {
            var left = row.RelativeItem().Text(label);
            var right = row.ConstantItem(70).AlignRight().Text(Money(amount));
            if (bold)
            {
                left.Bold();
                right.Bold();
            }
        });
    }

    private static string Money(decimal value) => "$" + value.ToString("0.00", Invariant);
}
