using AslanEtsy.Domain.Entities;
using AslanEtsy.Domain.Enums;
using AslanEtsy.Domain.Interfaces;
using AslanEtsy.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AslanEtsy.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByReceiptIdAsync(long receiptId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.Trackings)
            .Include(o => o.EtsyAccount)
            .FirstOrDefaultAsync(o => o.ReceiptId == receiptId, cancellationToken);
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Include(o => o.Trackings)
            .Include(o => o.EtsyAccount)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersFilteredAsync(
        int? etsyAccountId = null,
        OrderStatus? status = null,
        CustomOrderStatus? customStatus = null,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(etsyAccountId, status, customStatus, searchTerm, startDate, endDate);

        return await query
            .Include(o => o.Items)
            .Include(o => o.Trackings)
            .Include(o => o.EtsyAccount)
            .OrderByDescending(o => o.OrderDateUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetOrdersCountFilteredAsync(
        int? etsyAccountId = null,
        OrderStatus? status = null,
        CustomOrderStatus? customStatus = null,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(etsyAccountId, status, customStatus, searchTerm, startDate, endDate);
        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<Order> BuildFilterQuery(
        int? etsyAccountId,
        OrderStatus? status,
        CustomOrderStatus? customStatus,
        string? searchTerm,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _dbSet.AsQueryable();

        if (etsyAccountId.HasValue && etsyAccountId.Value > 0)
        {
            query = query.Where(o => o.EtsyAccountId == etsyAccountId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (customStatus.HasValue)
        {
            query = query.Where(o => o.CustomStatus == customStatus.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.OrderDateUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.OrderDateUtc <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(o =>
                o.ReceiptId.ToString().Contains(term) ||
                o.BuyerName.ToLower().Contains(term) ||
                o.BuyerEmail.ToLower().Contains(term) ||
                (o.RecipientName != null && o.RecipientName.ToLower().Contains(term)) ||
                (o.ShippingCity != null && o.ShippingCity.ToLower().Contains(term)) ||
                (o.Tags != null && o.Tags.ToLower().Contains(term)) ||
                (o.InternalNote != null && o.InternalNote.ToLower().Contains(term)) ||
                o.Items.Any(i => i.Title.ToLower().Contains(term) || (i.Sku != null && i.Sku.ToLower().Contains(term))) ||
                o.Trackings.Any(t => t.TrackingCode.ToLower().Contains(term)));
        }

        return query;
    }
}
