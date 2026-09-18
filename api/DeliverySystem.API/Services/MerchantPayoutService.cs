using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class MerchantPayoutService(DeliveryDbContext db)
{
    public async Task<List<MerchantPayoutResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var payouts = await db.MerchantPayouts
            .AsNoTracking()
            .OrderBy(item => item.MerchantPayoutId)
            .ToListAsync(cancellationToken);

        return payouts.Select(item => item.ToResponse()).ToList();
    }

    public async Task<MerchantPayoutResponse> CreateAsync(
        CreateMerchantPayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var method = request.PaymentMethod.Trim();
        if (method is not ("Cash" or "BankTransfer"))
        {
            throw new BusinessRuleException("Payment method must be Cash or BankTransfer.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var settlement = await db.MerchantSettlements.FirstOrDefaultAsync(
            item => item.MerchantSettlementId == request.MerchantSettlementId,
            cancellationToken);

        if (settlement is null)
        {
            throw new NotFoundException("Merchant settlement was not found.");
        }

        if (settlement.Status != "Approved")
        {
            throw new BusinessRuleException("Settlement is not approved.");
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

        var duplicateKey = await db.MerchantPayouts.AnyAsync(
            item => item.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);

        if (duplicateKey)
        {
            throw new ConflictException("This payout was already recorded.");
        }

        var duplicateReference = await db.MerchantPayouts.AnyAsync(
            item => item.PaymentReference == request.PaymentReference.Trim(),
            cancellationToken);

        if (duplicateReference)
        {
            throw new ConflictException("This payment reference already exists.");
        }

        var settlementTotal = await db.MerchantSettlementLines
            .Where(item => item.MerchantSettlementId == request.MerchantSettlementId)
            .SumAsync(item => (decimal?)item.SettlementAmount, cancellationToken) ?? 0;

        var alreadyPaid = await db.MerchantPayouts
            .Where(item => item.MerchantSettlementId == request.MerchantSettlementId)
            .SumAsync(item => (decimal?)item.Amount, cancellationToken) ?? 0;

        if (alreadyPaid + request.Amount > settlementTotal)
        {
            throw new BusinessRuleException("Payout exceeds remaining settlement amount.");
        }

        var now = DateTime.UtcNow;
        var payout = new MerchantPayout
        {
            MerchantSettlementId = request.MerchantSettlementId,
            IdempotencyKey = request.IdempotencyKey,
            PaymentReference = request.PaymentReference.Trim(),
            Amount = request.Amount,
            PaymentMethod = method,
            PaidAt = now,
            PaidBy = request.PaidBy.Trim(),
            ProofReference = string.IsNullOrWhiteSpace(request.ProofReference)
                ? null
                : request.ProofReference.Trim()
        };

        db.MerchantPayouts.Add(payout);
        await db.SaveChangesAsync(cancellationToken);

        db.CashMovements.Add(new CashMovement
        {
            CashAccountId = request.CashAccountId,
            SourceType = "MerchantPayout",
            MerchantPayoutId = payout.MerchantPayoutId,
            Direction = "OUT",
            Amount = request.Amount,
            OccurredAt = now,
            RecordedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return payout.ToResponse();
    }
}
