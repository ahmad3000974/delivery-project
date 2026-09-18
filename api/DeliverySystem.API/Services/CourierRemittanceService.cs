using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class CourierRemittanceService(DeliveryDbContext db)
{
    public async Task<List<CourierRemittanceResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var remittances = await db.CourierRemittances
            .AsNoTracking()
            .OrderBy(item => item.CourierRemittanceId)
            .ToListAsync(cancellationToken);

        return remittances.Select(item => item.ToResponse()).ToList();
    }

    public async Task<CourierRemittanceResponse> CreateAsync(
        CreateCourierRemittanceRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var courier = await db.Couriers.FirstOrDefaultAsync(
            item => item.CourierId == request.CourierId,
            cancellationToken);

        if (courier is null)
        {
            throw new NotFoundException("Courier was not found.");
        }

        if (!courier.IsActive)
        {
            throw new BusinessRuleException("Courier is not active.");
        }

        var cashAccount = await db.CashAccounts.FirstOrDefaultAsync(
            item => item.CashAccountId == request.CashAccountId,
            cancellationToken);

        if (cashAccount is null)
        {
            throw new NotFoundException("Cash account was not found.");
        }

        if (!cashAccount.IsActive)
        {
            throw new BusinessRuleException("Cash account is not active.");
        }

        var duplicateKey = await db.CourierRemittances.AnyAsync(
            item => item.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);

        if (duplicateKey)
        {
            throw new ConflictException("This remittance was already recorded.");
        }

        var now = DateTime.UtcNow;
        var remittance = new CourierRemittance
        {
            CourierId = request.CourierId,
            CashAccountId = request.CashAccountId,
            Amount = request.Amount,
            IdempotencyKey = request.IdempotencyKey,
            ReceivedBy = request.ReceivedBy.Trim(),
            ProofReference = string.IsNullOrWhiteSpace(request.ProofReference)
                ? null
                : request.ProofReference.Trim(),
            RemittedAt = now
        };

        db.CourierRemittances.Add(remittance);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var item in request.Allocations)
        {
            var collection = await db.Collections.FirstOrDefaultAsync(
                row => row.CollectionId == item.CollectionId,
                cancellationToken);

            if (collection is null)
            {
                throw new NotFoundException("Collection was not found.");
            }

            if (collection.CourierId != request.CourierId)
            {
                throw new BusinessRuleException("Collection does not belong to this courier.");
            }

            var alreadyAllocated = await db.RemittanceAllocations
                .Where(row => row.CollectionId == item.CollectionId)
                .SumAsync(row => (decimal?)row.AllocatedAmount, cancellationToken) ?? 0;

            var remaining = collection.Amount - alreadyAllocated;
            if (item.AllocatedAmount > remaining)
            {
                throw new BusinessRuleException("Allocated amount exceeds the collection remainder.");
            }

            db.RemittanceAllocations.Add(new RemittanceAllocation
            {
                CourierRemittanceId = remittance.CourierRemittanceId,
                CollectionId = item.CollectionId,
                AllocatedAmount = item.AllocatedAmount,
                AllocatedAt = now
            });
        }

        db.CashMovements.Add(new CashMovement
        {
            CashAccountId = request.CashAccountId,
            SourceType = "CourierRemittance",
            CourierRemittanceId = remittance.CourierRemittanceId,
            Direction = "IN",
            Amount = request.Amount,
            OccurredAt = now,
            RecordedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return remittance.ToResponse();
    }
}
