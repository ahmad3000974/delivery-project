using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class AdjustmentService(DeliveryDbContext db)
{
    public async Task<List<AdjustmentResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var adjustments = await db.Adjustments
            .AsNoTracking()
            .OrderBy(item => item.AdjustmentId)
            .ToListAsync(cancellationToken);

        return adjustments.Select(item => item.ToResponse()).ToList();
    }

    public async Task<AdjustmentResponse> CreateAsync(
        CreateAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var targetType = request.TargetType.Trim();
        if (targetType is not ("Collection" or "MerchantAccrual" or "CourierCommission" or "CashMovement"))
        {
            throw new BusinessRuleException(
                "Target type must be Collection, MerchantAccrual, CourierCommission, or CashMovement.");
        }

        if (request.AmountDelta == 0)
        {
            throw new BusinessRuleException("Amount delta cannot be zero.");
        }

        var duplicateKey = await db.Adjustments.AnyAsync(
            item => item.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);

        if (duplicateKey)
        {
            throw new ConflictException("This adjustment was already recorded.");
        }

        await EnsureTargetExistsAsync(targetType, request.OriginalId, cancellationToken);

        var adjustment = new Adjustment
        {
            TargetType = targetType,
            OriginalCollectionId = targetType == "Collection" ? request.OriginalId : null,
            OriginalAccrualId = targetType == "MerchantAccrual" ? request.OriginalId : null,
            OriginalCommissionId = targetType == "CourierCommission" ? request.OriginalId : null,
            OriginalCashMovementId = targetType == "CashMovement" ? request.OriginalId : null,
            IdempotencyKey = request.IdempotencyKey,
            AmountDelta = request.AmountDelta,
            Reason = request.Reason.Trim(),
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy.Trim()
        };

        db.Adjustments.Add(adjustment);
        await db.SaveChangesAsync(cancellationToken);
        return adjustment.ToResponse();
    }

    public async Task<AdjustmentResponse> ApproveAsync(
        long adjustmentId,
        ApproveAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var adjustment = await db.Adjustments.FirstOrDefaultAsync(
            item => item.AdjustmentId == adjustmentId,
            cancellationToken);

        if (adjustment is null)
        {
            throw new NotFoundException("Adjustment was not found.");
        }

        if (adjustment.Status != "Draft")
        {
            throw new BusinessRuleException("Only a draft adjustment can be approved.");
        }

        var now = DateTime.UtcNow;
        adjustment.Status = "Approved";
        adjustment.ApprovedAt = now;
        adjustment.ApprovedBy = request.ApprovedBy.Trim();

        if (adjustment.TargetType == "CashMovement" && adjustment.OriginalCashMovementId is long movementId)
        {
            await AddCashCorrectionAsync(adjustment, movementId, adjustment.AmountDelta, now, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return adjustment.ToResponse();
    }

    public async Task<AdjustmentResponse> ReverseAsync(
        long adjustmentId,
        ReverseAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var original = await db.Adjustments.FirstOrDefaultAsync(
            item => item.AdjustmentId == adjustmentId,
            cancellationToken);

        if (original is null)
        {
            throw new NotFoundException("Adjustment was not found.");
        }

        if (original.Status != "Approved")
        {
            throw new BusinessRuleException("Only an approved adjustment can be reversed.");
        }

        var alreadyReversed = await db.Adjustments.AnyAsync(
            item => item.ReversesAdjustmentId == original.AdjustmentId,
            cancellationToken);

        if (alreadyReversed)
        {
            throw new ConflictException("This adjustment was already reversed.");
        }

        var now = DateTime.UtcNow;
        original.Status = "Reversed";

        var reversal = new Adjustment
        {
            TargetType = original.TargetType,
            OriginalCollectionId = original.OriginalCollectionId,
            OriginalAccrualId = original.OriginalAccrualId,
            OriginalCommissionId = original.OriginalCommissionId,
            OriginalCashMovementId = original.OriginalCashMovementId,
            ReversesAdjustmentId = original.AdjustmentId,
            IdempotencyKey = Guid.NewGuid(),
            AmountDelta = -original.AmountDelta,
            Reason = $"Reversal of adjustment {original.AdjustmentId}",
            Status = "Approved",
            CreatedAt = now,
            CreatedBy = request.CreatedBy.Trim(),
            ApprovedAt = now,
            ApprovedBy = request.CreatedBy.Trim()
        };

        db.Adjustments.Add(reversal);
        await db.SaveChangesAsync(cancellationToken);

        if (reversal.TargetType == "CashMovement" && reversal.OriginalCashMovementId is long movementId)
        {
            await AddCashCorrectionAsync(reversal, movementId, reversal.AmountDelta, now, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return reversal.ToResponse();
    }

    private async Task EnsureTargetExistsAsync(
        string targetType,
        long originalId,
        CancellationToken cancellationToken)
    {
        var exists = targetType switch
        {
            "Collection" => await db.Collections.AnyAsync(
                item => item.CollectionId == originalId, cancellationToken),
            "MerchantAccrual" => await db.MerchantAccruals.AnyAsync(
                item => item.MerchantAccrualId == originalId, cancellationToken),
            "CourierCommission" => await db.CourierCommissions.AnyAsync(
                item => item.CourierCommissionId == originalId, cancellationToken),
            "CashMovement" => await db.CashMovements.AnyAsync(
                item => item.CashMovementId == originalId, cancellationToken),
            _ => false
        };

        if (!exists)
        {
            throw new NotFoundException("Adjustment target was not found.");
        }
    }

    private async Task AddCashCorrectionAsync(
        Adjustment adjustment,
        long originalCashMovementId,
        decimal amountDelta,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var originalMovement = await db.CashMovements.FirstOrDefaultAsync(
            item => item.CashMovementId == originalCashMovementId,
            cancellationToken);

        if (originalMovement is null)
        {
            throw new NotFoundException("Cash movement was not found.");
        }

        db.CashMovements.Add(new CashMovement
        {
            CashAccountId = originalMovement.CashAccountId,
            SourceType = "Adjustment",
            AdjustmentId = adjustment.AdjustmentId,
            Direction = amountDelta > 0 ? "IN" : "OUT",
            Amount = Math.Abs(amountDelta),
            OccurredAt = now,
            RecordedAt = now
        });
    }
}
