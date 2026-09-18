using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryOrdersController(OrderService orders) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DeliveryOrderResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return await orders.ListOrdersAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryOrderResponse>> Create(
        CreateDeliveryOrderRequest request,
        CancellationToken cancellationToken)
    {
        return await orders.CreateOrderAsync(request, cancellationToken);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<DeliveryOrderResponse>> UpdateStatus(
        long id,
        UpdateDeliveryOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        return await orders.UpdateOrderStatusAsync(id, request, cancellationToken);
    }
}
