using AslanEtsy.Domain.Entities;
using AslanEtsy.Domain.Enums;

namespace AslanEtsy.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByReceiptIdAsync(long receiptId, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersFilteredAsync(
        int? etsyAccountId = null,
        OrderStatus? status = null,
        CustomOrderStatus? customStatus = null,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<int> GetOrdersCountFilteredAsync(
        int? etsyAccountId = null,
        OrderStatus? status = null,
        CustomOrderStatus? customStatus = null,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
