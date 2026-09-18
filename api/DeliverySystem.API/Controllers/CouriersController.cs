using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouriersController(CourierService couriers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CourierResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return await couriers.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<CourierResponse>> Create(
        CreateCourierRequest request,
        CancellationToken cancellationToken)
    {
        return await couriers.CreateAsync(request, cancellationToken);
    }
}
