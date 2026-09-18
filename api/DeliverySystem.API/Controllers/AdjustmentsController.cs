using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdjustmentsController(AdjustmentService adjustments) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AdjustmentResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return await adjustments.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<AdjustmentResponse>> Create(
        CreateAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        return await adjustments.CreateAsync(request, cancellationToken);
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<AdjustmentResponse>> Approve(
        long id,
        ApproveAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        return await adjustments.ApproveAsync(id, request, cancellationToken);
    }

    [HttpPut("{id}/reverse")]
    public async Task<ActionResult<AdjustmentResponse>> Reverse(
        long id,
        ReverseAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        return await adjustments.ReverseAsync(id, request, cancellationToken);
    }
}
