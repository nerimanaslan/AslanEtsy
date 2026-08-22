using AslanEtsy.Application.DTOs.Common;
using AslanEtsy.Application.DTOs.Orders;
using AslanEtsy.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AslanEtsy.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders([FromQuery] OrderFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _orderService.GetOrdersAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order == null) return NotFound(new { message = $"Sipariş bulunamadı (ID: {id})" });
        return Ok(order);
    }

    [HttpGet("receipt/{receiptId:long}")]
    public async Task<ActionResult<OrderDetailDto>> GetByReceiptId(long receiptId, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByReceiptIdAsync(receiptId, cancellationToken);
        if (order == null) return NotFound(new { message = $"Etsy siparişi bulunamadı (Receipt ID: {receiptId})" });
        return Ok(order);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> UpdateOrder(int id, [FromBody] UpdateOrderDto dto, CancellationToken cancellationToken)
    {
        var updated = await _orderService.UpdateOrderDetailsAsync(id, dto, cancellationToken);
        if (updated == null) return NotFound(new { message = $"Sipariş bulunamadı (ID: {id})" });
        return Ok(updated);
    }

    [HttpPost("{id:int}/tracking")]
    public async Task<ActionResult<OrderTrackingDto>> AddTracking(int id, [FromBody] CreateOrderTrackingDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.TrackingCode) || string.IsNullOrWhiteSpace(dto.CarrierName))
        {
            return BadRequest(new { message = "Takip numarası ve Kargo firması adı zorunludur." });
        }

        var tracking = await _orderService.AddTrackingAsync(id, dto, cancellationToken);
        if (tracking == null) return NotFound(new { message = $"Sipariş bulunamadı (ID: {id})" });
        return Ok(tracking);
    }

    [HttpPost("tracking/{trackingId:int}/resync")]
    public async Task<IActionResult> ResyncTracking(int trackingId, CancellationToken cancellationToken)
    {
        var success = await _orderService.ResyncTrackingToEtsyAsync(trackingId, cancellationToken);
        if (success)
        {
            return Ok(new { message = "Kargo bilgisi Etsy'ye başarıyla iletildi." });
        }
        return BadRequest(new { message = "Kargo bilgisi Etsy'ye iletilemedi. Lütfen bağlantıyı ve mağaza durumunu kontrol edin." });
    }

    [HttpDelete("tracking/{trackingId:int}")]
    public async Task<IActionResult> DeleteTracking(int trackingId, CancellationToken cancellationToken)
    {
        var success = await _orderService.DeleteTrackingAsync(trackingId, cancellationToken);
        if (!success) return NotFound(new { message = $"Kargo kaydı bulunamadı (ID: {trackingId})" });
        return NoContent();
    }
}
