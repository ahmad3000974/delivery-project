using Microsoft.AspNetCore.Mvc;
using DeliverySystem.API.Services;
using DeliverySystem.API.Models;
namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourierRemittancesController : ControllerBase
{
    private readonly CourierRemittanceService _courierRemittanceService;
    public CourierRemittancesController(CourierRemittanceService courierRemittanceService){

        _courierRemittanceService = courierRemittanceService;
    }
    [HttpGet]
    public async Task<ActionResult<List<CourierRemittanceResponse>>> GetAll(CancellationToken cancellationToken)
{
    return await _courierRemittanceService.ListAsync(cancellationToken);
}
[HttpPost]
public async Task<ActionResult<CourierRemittanceResponse>> Create(CreateCourierRemittanceRequest request, CancellationToken cancellationToken)
{
    return await _courierRemittanceService.CreateAsync(request, cancellationToken);
}
    
}
