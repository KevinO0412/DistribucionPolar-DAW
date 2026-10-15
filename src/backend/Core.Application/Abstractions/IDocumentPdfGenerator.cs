using Core.Domain.Entities;

namespace Core.Application.Abstractions;

/// <summary>Genera documentos imprimibles en PDF.</summary>
public interface IDocumentPdfGenerator
{
    /// <summary>Ticket de factura o regalía.</summary>
    byte[] RenderInvoiceTicket(Invoice invoice, Client? client);

    /// <summary>Liquidación de un evento a consignación.</summary>
    byte[] RenderConsignment(ConsignmentEvent consignmentEvent);
}
