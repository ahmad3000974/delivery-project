using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantsController(MerchantService merchants) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MerchantResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return await merchants.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<MerchantResponse>> Create(
        CreateMerchantRequest request,
        CancellationToken cancellationToken)
    {
        return await merchants.CreateAsync(request, cancellationToken);
    }
}
