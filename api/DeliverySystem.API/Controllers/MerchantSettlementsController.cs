using DeliverySystem.API.Models;
using DeliverySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantSettlementsController(MerchantSettlementService settlements) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MerchantSettlementResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return await settlements.ListAsync(cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<MerchantSettlementResponse>> Create(
        CreateMerchantSettlementRequest request,
        CancellationToken cancellationToken)
    {
        return await settlements.CreateAsync(request, cancellationToken);
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<MerchantSettlementResponse>> Approve(
        long id,
        ApproveMerchantSettlementRequest request,
        CancellationToken cancellationToken)
    {
        return await settlements.ApproveAsync(id, request, cancellationToken);
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<MerchantSettlementResponse>> Cancel(
        long id,
        CancellationToken cancellationToken)
    {
        return await settlements.CancelAsync(id, cancellationToken);
    }
}
