using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderAssignmentsController(OrderService orders) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OrderAssignmentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return await orders.ListAssignmentsAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<OrderAssignmentResponse>> Create(
        CreateOrderAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        return await orders.AssignAsync(request, cancellationToken);
    }
}
