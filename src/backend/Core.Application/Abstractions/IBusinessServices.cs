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

    Task<TruckResponse> DispatchAsync(Guid truckId, CancellationToken cancellationToken = default);

    Task<RouteSettlementResponse> SettleAsync(Guid truckId, SettleRouteRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RouteSettlementResponse>> GetSettlementsAsync(Guid? truckId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);

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

    Task<ClientResponse> SetCreditLimitAsync(Guid id, SetCreditLimitRequest request, CancellationToken cancellationToken = default);
}

public interface IInvoiceService
{
    Task<AttendantOptionsResponse> GetAttendantOptionsAsync(CancellationToken cancellationToken = default);

    Task<InvoiceResponse> CreateSaleAsync(CreateSaleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Venta idempotente: si ya existe una factura con <paramref name="idempotencyKey"/>, la devuelve sin crear otra
    /// (Created = false). Es lo que usa la preventa sin conexión al sincronizar.
    /// </summary>
    Task<(InvoiceResponse Invoice, bool Created)> CreateSaleAsync(
        CreateSaleRequest request, Guid? idempotencyKey, CancellationToken cancellationToken = default);

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
