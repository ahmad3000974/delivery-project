using Microsoft.AspNetCore.Mvc;
using DeliverySystem.API.Models;
using DeliverySystem.API.Services;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourierCommissionPaymentsController(CourierCommissionPaymentService payments) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CourierCommissionPaymentResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return await payments.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<CourierCommissionPaymentResponse>> Create(
        CreateCourierCommissionPaymentRequest request,
        CancellationToken cancellationToken)
    {
        return await payments.CreateAsync(request, cancellationToken);
    }
}
