using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class MerchantSettlementService
{
    private readonly DeliveryDbContext _db;
    public MerchantSettlementService(DeliveryDbContext db){
        _db = db;
    }
    public async Task<List<MerchantSettlementResponse>> ListAsync(CancellationToken cancellationToken)
{
    var settlements = await _db.MerchantSettlements
        .AsNoTracking()
        .OrderBy(item => item.MerchantSettlementId)
        .ToListAsync(cancellationToken);

    return settlements.Select(item => item.ToResponse()).ToList();
}

    public async Task<MerchantSettlementResponse> CreateAsync(
        CreateMerchantSettlementRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var merchant = await _db.Merchants.FirstOrDefaultAsync(
            item => item.MerchantId == request.MerchantId,
            cancellationToken);
    
        if (merchant is null)
        {
            throw new NotFoundException("Merchant was not found.");
        }

        if (!merchant.IsActive)
        {
            throw new BusinessRuleException("Merchant is not active.");
        }

        var requestedIds = request.MerchantAccrualIds.Distinct().ToList();

        var accruals = await _db.MerchantAccruals
            .Include(item => item.Order)
            .Where(item => requestedIds.Contains(item.MerchantAccrualId))
            .ToListAsync(cancellationToken);

        if (accruals.Count != requestedIds.Count)
        {
            throw new NotFoundException("Merchant accrual was not found.");
        }

        var alreadyOpen = await _db.MerchantSettlementLines.AnyAsync(
            line => requestedIds.Contains(line.MerchantAccrualId) && line.ReleasedAt == null,
            cancellationToken);

        if (alreadyOpen)
        {
            throw new ConflictException("This accrual is already on an open settlement.");
        }

        foreach (var accrual in accruals)
        {
            if (accrual.Order.MerchantId != request.MerchantId)
            {
                throw new BusinessRuleException("Accrual does not belong to this merchant.");
            }

            if (accrual.MerchantAmount <= 0)
            {
                throw new BusinessRuleException("Settlement amount must be greater than zero.");
            }
        }

        var now = DateTime.UtcNow;
        var settlement = new MerchantSettlement
        {
            MerchantId = request.MerchantId,
            Status = "Draft",
            CreatedAt = now,
            CreatedBy = request.CreatedBy.Trim()
        };

        _db.MerchantSettlements.Add(settlement);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var accrual in accruals)
        {
            _db.MerchantSettlementLines.Add(new MerchantSettlementLine
            {
                MerchantSettlementId = settlement.MerchantSettlementId,
                MerchantAccrualId = accrual.MerchantAccrualId,
                SettlementAmount = accrual.MerchantAmount
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return settlement.ToResponse();
    }

    public async Task<MerchantSettlementResponse> ApproveAsync(
        long merchantSettlementId,
        ApproveMerchantSettlementRequest request,
        CancellationToken cancellationToken)
    {
        var settlement = await _db.MerchantSettlements.FirstOrDefaultAsync(
            item => item.MerchantSettlementId == merchantSettlementId,
            cancellationToken);

        if (settlement is null)
        {
            throw new NotFoundException("Merchant settlement was not found.");
        }

        if (settlement.Status != "Draft")
        {
            throw new BusinessRuleException("Only a draft settlement can be approved.");
        }

        settlement.Status = "Approved";
        settlement.ApprovedAt = DateTime.UtcNow;
        settlement.ApprovedBy = request.ApprovedBy.Trim();

        await _db.SaveChangesAsync(cancellationToken);
        return settlement.ToResponse();
    }

    public async Task<MerchantSettlementResponse> CancelAsync(
        long merchantSettlementId,
        CancellationToken cancellationToken)
    {
        var settlement = await _db.MerchantSettlements
            .Include(item => item.MerchantSettlementLines)
            .FirstOrDefaultAsync(
                item => item.MerchantSettlementId == merchantSettlementId,
                cancellationToken);

        if (settlement is null)
        {
            throw new NotFoundException("Merchant settlement was not found.");
        }

        if (settlement.Status != "Draft")
        {
            throw new BusinessRuleException("Only a draft settlement can be cancelled.");
        }

        var now = DateTime.UtcNow;
        settlement.Status = "Cancelled";
        foreach (var line in settlement.MerchantSettlementLines)
        {
            line.ReleasedAt ??= now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return settlement.ToResponse();
    }
}
