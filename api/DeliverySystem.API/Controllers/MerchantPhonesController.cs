using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantPhonesController(MerchantPhoneService phones) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MerchantPhoneResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return await phones.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<MerchantPhoneResponse>> Create(
        CreateMerchantPhoneRequest request,
        CancellationToken cancellationToken)
    {
        return await phones.CreateAsync(request, cancellationToken);
    }
}
