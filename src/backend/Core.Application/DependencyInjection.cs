using System.Globalization;
using Core.Application.Abstractions;
using Core.Application.Services;
using Core.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("es");
        ValidatorOptions.Global.DisplayNameResolver = SpanishDisplayNames.Resolve;

        // Validadores sin estado: una sola instancia para toda la aplicación.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton);

        // Servicios de aplicación: una instancia por request HTTP (comparten el DbContext del request).
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ITruckService, TruckService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IStockWithdrawalService, StockWithdrawalService>();
        services.AddScoped<IConsignmentService, ConsignmentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
