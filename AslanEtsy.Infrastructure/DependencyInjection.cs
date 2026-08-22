using AslanEtsy.Application.Interfaces;
using AslanEtsy.Application.Services;
using AslanEtsy.Domain.Interfaces;
using AslanEtsy.Infrastructure.BackgroundServices;
using AslanEtsy.Infrastructure.Context;
using AslanEtsy.Infrastructure.EtsyApi;
using AslanEtsy.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AslanEtsy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // SQLite Database connection
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Data Source=aslan_etsy.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        // Repositories & UnitOfWork
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEtsyAccountRepository, EtsyAccountRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Etsy HTTP Client & API Client
        services.AddHttpClient<IEtsyApiClient, EtsyApiClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Application Services
        services.AddScoped<IEtsyAccountService, EtsyAccountService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IEtsySyncService, EtsySyncService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Background Worker for Auto-Sync
        services.AddHostedService<EtsyOrderSyncBackgroundService>();

        return services;
    }
}
