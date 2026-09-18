using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantPayoutsController(MerchantPayoutService payouts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MerchantPayoutResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return await payouts.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<MerchantPayoutResponse>> Create(
        CreateMerchantPayoutRequest request,
        CancellationToken cancellationToken)
    {
        return await payouts.CreateAsync(request, cancellationToken);
    }
}
