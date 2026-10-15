using Core.Application.DTOs;
using Core.Domain.Enums;

namespace Core.Application.Abstractions;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);

    Task<EmployeeResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
}

public interface ITruckService
{
    Task<IReadOnlyList<TruckResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TruckResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TruckResponse> CreateAsync(TruckRequest request, CancellationToken cancellationToken = default);

    Task<TruckResponse> UpdateAsync(Guid id, TruckRequest request, CancellationToken cancellationToken = default);

    Task<TruckResponse> LoadAsync(Guid truckId, LoadTruckRequest request, CancellationToken cancellationToken = default);

    Task<TruckResponse> TransferAsync(Guid truckId, TransferFromTruckRequest request, CancellationToken cancellationToken = default);

    Task<TruckResponse> AdjustAsync(Guid truckId, AdjustStockRequest request, CancellationToken cancellationToken = default);

    Task<TruckResponse> UnloadAsync(Guid truckId, CancellationToken cancellationToken = default);

    /// <summary>Despacha el camión a ruta (Cargando → En ruta).</summary>
    Task<TruckResponse> DispatchAsync(Guid truckId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TruckLoadResponse>> GetLoadsAsync(Guid? truckId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}

public interface IInventoryService
{
    Task<ProductResponse> AdjustWarehouseStockAsync(Guid productId, AdjustWarehouseStockRequest request, CancellationToken cancellationToken = default);

    Task<InventorySummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<ProductLocationResponse> GetProductLocationAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IClientService
{
    Task<IReadOnlyList<ClientResponse>> SearchAsync(string? search, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClientResponse>> GetReceivablesAsync(CancellationToken cancellationToken = default);

    Task<ClientResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientStatementResponse> GetStatementAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientResponse> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken = default);

    Task<ClientResponse> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClientResponse> RegisterPaymentAsync(Guid id, ClientPaymentRequest request, CancellationToken cancellationToken = default);

    Task<ClientResponse> ReturnEmptiesAsync(Guid id, ClientEmptiesReturnRequest request, CancellationToken cancellationToken = default);

    Task<ClientResponse> AdjustBalancesAsync(Guid id, ClientBalanceAdjustmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Situación de crédito y envejecimiento de la deuda del cliente.</summary>
    Task<CreditStatusResponse> GetCreditStatusAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Fija el límite y los días de crédito (solo Admin).</summary>
    Task<CreditStatusResponse> SetCreditTermsAsync(Guid id, CreditTermsRequest request, CancellationToken cancellationToken = default);
}

public interface IInvoiceService
{
    Task<AttendantOptionsResponse> GetAttendantOptionsAsync(CancellationToken cancellationToken = default);

    Task<InvoiceResponse> CreateSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Venta idempotente: si la clave ya se usó con el mismo cuerpo devuelve la factura emitida (sin tocar el stock);
    /// si se usó con otro cuerpo lanza <see cref="Exceptions.IdempotencyKeyReusedException"/>.
    /// </summary>
    Task<SaleResult> CreateSaleAsync(CreateSaleRequest request, string idempotencyKey, CancellationToken cancellationToken = default);

    Task<InvoiceResponse> CreateGiftAsync(CreateGiftRequest request, CancellationToken cancellationToken = default);

    Task<InvoiceResponse> CancelAsync(Guid id, CancelInvoiceRequest request, CancellationToken cancellationToken = default);

    Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResponse<InvoiceSummaryResponse>> ListAsync(
        DateOnly? from, DateOnly? to, Guid? clientId, InvoiceType? type, bool includeCancelled, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<byte[]> GetTicketPdfAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductResponse>> GetGiftEligibleProductsAsync(CancellationToken cancellationToken = default);
}

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, CancellationToken cancellationToken = default);

    Task<PurchaseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseResponse>> ListAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}

public interface IStockWithdrawalService
{
    Task<DamageResponse> RegisterDamageAsync(CreateDamageRequest request, CancellationToken cancellationToken = default);

    Task<WithdrawalListResponse<DamageResponse>> ListDamagesAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);

    Task<InternalConsumptionResponse> RegisterConsumptionAsync(CreateInternalConsumptionRequest request, CancellationToken cancellationToken = default);

    Task<WithdrawalListResponse<InternalConsumptionResponse>> ListConsumptionsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
}

public interface IConsignmentService
{
    Task<ConsignmentResponse> CreateAsync(CreateConsignmentRequest request, CancellationToken cancellationToken = default);

    Task<ConsignmentResponse> RegisterReturnAsync(Guid id, ConsignmentReturnRequest request, CancellationToken cancellationToken = default);

    Task<ConsignmentResponse> CloseAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ConsignmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConsignmentResponse>> ListAsync(bool? isClosed, CancellationToken cancellationToken = default);

    Task<ConsignmentIndicatorsResponse> GetIndicatorsAsync(CancellationToken cancellationToken = default);

    Task<byte[]> GetPdfAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IReportService
{
    Task<DailyReportResponse> GetDailyReportAsync(DateOnly? date, CancellationToken cancellationToken = default);

    Task<BalanceResponse> GetBalanceAsync(CancellationToken cancellationToken = default);

    Task<DashboardResponse> GetDashboardAsync(DateOnly? from, DateOnly? to, Guid? categoryId, CancellationToken cancellationToken = default);

    Task<PagedResponse<AuditLogResponse>> GetAuditLogAsync(
        DateOnly? from, DateOnly? to, string? module, string? username, int page, int pageSize, CancellationToken cancellationToken = default);
}

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(Guid id, ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task ChangeOwnPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
