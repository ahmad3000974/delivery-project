using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryAttemptsController(DeliveryAttemptService attempts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DeliveryAttemptResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return await attempts.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryAttemptResponse>> Create(
        CreateDeliveryAttemptRequest request,
        CancellationToken cancellationToken)
    {
        return await attempts.CreateAsync(request, cancellationToken);
    }
}
