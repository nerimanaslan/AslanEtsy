using AslanEtsy.Application.DTOs.Common;
using AslanEtsy.Application.DTOs.Orders;

namespace AslanEtsy.Application.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetOrdersAsync(OrderFilterRequest filter, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetOrderByReceiptIdAsync(long receiptId, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> UpdateOrderDetailsAsync(int id, UpdateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderTrackingDto?> AddTrackingAsync(int orderId, CreateOrderTrackingDto dto, CancellationToken cancellationToken = default);
    Task<bool> ResyncTrackingToEtsyAsync(int trackingId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTrackingAsync(int trackingId, CancellationToken cancellationToken = default);
}
