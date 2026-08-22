using AslanEtsy.Application.DTOs.Orders;

namespace AslanEtsy.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalShops { get; set; }
    public int ActiveShops { get; set; }
    public int TotalOrders { get; set; }
    public int OpenOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int UnfulfilledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    
    public List<ShopOrderSummaryDto> ShopSummaries { get; set; } = new();
    public List<OrderDto> RecentOrders { get; set; } = new();
}

public class ShopOrderSummaryDto
{
    public int AccountId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int OpenOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime? LastSyncAtUtc { get; set; }
    public bool IsConnected { get; set; }
}
