using AslanEtsy.Application.DTOs.Dashboard;
using AslanEtsy.Application.DTOs.Orders;
using AslanEtsy.Application.Interfaces;
using AslanEtsy.Domain.Enums;
using AslanEtsy.Domain.Interfaces;

namespace AslanEtsy.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _unitOfWork.Accounts.GetAllAsync(cancellationToken);
        var activeAccounts = accounts.Where(a => a.IsActive).ToList();

        var orders = await _unitOfWork.Orders.GetOrdersFilteredAsync(
            pageNumber: 1,
            pageSize: 5000,
            cancellationToken: cancellationToken);

        var totalOrders = orders.Count;
        var openOrders = orders.Count(o => !o.IsShipped && o.Status != OrderStatus.Canceled && o.Status != OrderStatus.Refunded);
        var shippedOrders = orders.Count(o => o.IsShipped || o.Status == OrderStatus.Shipped);
        var unfulfilledOrders = orders.Count(o => o.IsPaid && !o.IsShipped);
        var totalRevenue = orders.Where(o => o.IsPaid).Sum(o => o.GrandTotalAmount);

        var shopSummaries = new List<ShopOrderSummaryDto>();
        foreach (var account in accounts)
        {
            var shopOrders = orders.Where(o => o.EtsyAccountId == account.Id).ToList();
            shopSummaries.Add(new ShopOrderSummaryDto
            {
                AccountId = account.Id,
                ShopName = account.ShopName,
                TotalOrders = shopOrders.Count,
                OpenOrders = shopOrders.Count(o => !o.IsShipped && o.Status != OrderStatus.Canceled),
                TotalRevenue = shopOrders.Where(o => o.IsPaid).Sum(o => o.GrandTotalAmount),
                LastSyncAtUtc = account.LastSyncAtUtc,
                IsConnected = !string.IsNullOrWhiteSpace(account.AccessToken)
            });
        }

        var recentOrders = orders
            .OrderByDescending(o => o.OrderDateUtc)
            .Take(10)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                EtsyAccountId = o.EtsyAccountId,
                ShopName = o.EtsyAccount?.ShopName ?? "Mağaza #" + o.EtsyAccountId,
                ReceiptId = o.ReceiptId,
                BuyerName = o.BuyerName,
                BuyerEmail = o.BuyerEmail,
                GrandTotalAmount = o.GrandTotalAmount,
                CurrencyCode = o.CurrencyCode,
                IsPaid = o.IsPaid,
                IsShipped = o.IsShipped,
                Status = o.Status,
                CustomStatus = o.CustomStatus,
                OrderDateUtc = o.OrderDateUtc,
                ShippingCountryIso = o.ShippingCountryIso,
                ItemCount = o.Items?.Count ?? 0,
                HasTracking = o.Trackings?.Any(t => !t.IsDeleted) ?? false
            })
            .ToList();

        return new DashboardStatsDto
        {
            TotalShops = accounts.Count,
            ActiveShops = activeAccounts.Count,
            TotalOrders = totalOrders,
            OpenOrders = openOrders,
            ShippedOrders = shippedOrders,
            UnfulfilledOrders = unfulfilledOrders,
            TotalRevenue = totalRevenue,
            ShopSummaries = shopSummaries,
            RecentOrders = recentOrders
        };
    }
}
