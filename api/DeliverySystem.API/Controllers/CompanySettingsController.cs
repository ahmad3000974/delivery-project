using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanySettingsController(CompanySettingService settings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CompanySettingResponse>> Get(CancellationToken cancellationToken)
    {
        return await settings.GetAsync(cancellationToken);
    }

    [HttpPut]
    public async Task<ActionResult<CompanySettingResponse>> Update(
        UpdateCompanySettingRequest request,
        CancellationToken cancellationToken)
    {
        return await settings.UpdateAsync(request, cancellationToken);
    }
}
