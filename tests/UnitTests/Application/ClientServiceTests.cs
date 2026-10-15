using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Services;
using Core.Application.Validators;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentValidation;

namespace UnitTests.Application;

/// <summary>Abonos de dinero: saldo con signo (negativo = el cliente debe).</summary>
public sealed class ClientServiceTests
{
    private readonly Mock<IClientRepository> mockClients = new();
    private readonly Mock<IUnitOfWork> mockUnitOfWork = TestDoubles.UnitOfWork();
    private readonly Mock<IAuditService> mockAudit = new();
    private readonly ClientService service;
    private readonly Client client = new(1, "J-301798740", "Bodega La Esquina", null, "La Esquina", null);

    public ClientServiceTests()
    {
        service = new ClientService(
            mockClients.Object, Mock.Of<IDocumentNumberGenerator>(), mockUnitOfWork.Object, mockAudit.Object,
            new CreateClientRequestValidator(), new UpdateClientRequestValidator(), new ClientPaymentRequestValidator(),
            new ClientEmptiesReturnRequestValidator(), new ClientBalanceAdjustmentRequestValidator(), new SetCreditLimitRequestValidator());

        mockClients.Setup(r => r.GetForUpdateAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        mockClients.Setup(r => r.GetByIdAsync(client.Id, It.IsAny<CancellationToken>())).ReturnsAsync(client);
        mockClients.Setup(r => r.GetLastSaleDatesAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, DateTime>());
    }

    [Fact]
    public async Task RegisterPaymentAsync_ReducesDebt_AndRecordsMovement()
    {
        // Arrange: saldo −60 (el cliente debe 60).
        client.AdjustBalances(-60m, 0, 0);

        // Act: abona 25.
        var response = await service.RegisterPaymentAsync(client.Id, new ClientPaymentRequest(25m, "Pago en efectivo"));

        // Assert: saldo −35.
        Assert.Equal(-35m, response.MoneyBalance);
        Assert.True(response.IsReceivable);
        mockClients.Verify(r => r.AddMovement(It.Is<ClientMovement>(m =>
            m.Type == ClientMovementType.Payment && m.Amount == 25m && m.Description.Contains("Pago en efectivo"))), Times.Once);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "client_payment" && e.Amount == 25m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPaymentAsync_GreaterThanDebt_LeavesBalanceInFavor()
    {
        client.AdjustBalances(-60m, 0, 0);

        var response = await service.RegisterPaymentAsync(client.Id, new ClientPaymentRequest(80m, null));

        Assert.Equal(20m, response.MoneyBalance);
        Assert.False(response.IsReceivable);
    }

    [Fact]
    public async Task RegisterPaymentAsync_WithZeroAmount_ThrowsValidation_AndChangesNothing()
    {
        await Assert.ThrowsAsync<ValidationException>(() => service.RegisterPaymentAsync(client.Id, new ClientPaymentRequest(0m, null)));

        Assert.Equal(0m, client.MoneyBalance);
        mockClients.Verify(r => r.AddMovement(It.IsAny<ClientMovement>()), Times.Never);
    }

    [Fact]
    public async Task SetCreditLimitAsync_StoresTheLimitAndAudits()
    {
        var response = await service.SetCreditLimitAsync(client.Id, new SetCreditLimitRequest(250m));

        Assert.Equal(250m, response.CreditLimit);
        mockAudit.Verify(a => a.RecordAsync(It.Is<AuditEntry>(e => e.Action == "client_credit_limit"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCreditLimitAsync_Negative_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() => service.SetCreditLimitAsync(client.Id, new SetCreditLimitRequest(-1m)));
        Assert.Null(client.CreditLimit);
    }

    [Fact]
    public async Task RegisterPaymentAsync_UnknownClient_ThrowsKeyNotFound()
    {
        var unknown = Guid.NewGuid();
        mockClients.Setup(r => r.GetForUpdateAsync(unknown, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RegisterPaymentAsync(unknown, new ClientPaymentRequest(10m, null)));
        mockAudit.Verify(a => a.RecordAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
