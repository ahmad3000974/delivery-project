using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class CourierCommissionPaymentService(DeliveryDbContext db)
{
    public async Task<List<CourierCommissionPaymentResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var payments = await db.CourierCommissionPayments
            .AsNoTracking()
            .OrderBy(item => item.CourierCommissionPaymentId)
            .ToListAsync(cancellationToken);

        return payments.Select(item => item.ToResponse()).ToList();
    }

    public async Task<CourierCommissionPaymentResponse> CreateAsync(
        CreateCourierCommissionPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var method = request.PaymentMethod.Trim();
        if (method is not ("Cash" or "BankTransfer"))
        {
            throw new BusinessRuleException("Payment method must be Cash or BankTransfer.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var commission = await db.CourierCommissions.FirstOrDefaultAsync(
            item => item.CourierCommissionId == request.CourierCommissionId,
            cancellationToken);

        if (commission is null)
        {
            throw new NotFoundException("Courier commission was not found.");
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

        var duplicateKey = await db.CourierCommissionPayments.AnyAsync(
            item => item.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);

        if (duplicateKey)
        {
            throw new ConflictException("This payment was already recorded.");
        }

        var duplicateReference = await db.CourierCommissionPayments.AnyAsync(
            item => item.PaymentReference == request.PaymentReference.Trim(),
            cancellationToken);

        if (duplicateReference)
        {
            throw new ConflictException("This payment reference already exists.");
        }

        var alreadyPaid = await db.CourierCommissionPayments
            .Where(item => item.CourierCommissionId == request.CourierCommissionId)
            .SumAsync(item => (decimal?)item.Amount, cancellationToken) ?? 0;

        if (alreadyPaid + request.Amount > commission.Amount)
        {
            throw new BusinessRuleException("Payment exceeds remaining commission.");
        }

        var now = DateTime.UtcNow;
        var payment = new CourierCommissionPayment
        {
            CourierCommissionId = request.CourierCommissionId,
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

        db.CourierCommissionPayments.Add(payment);
        await db.SaveChangesAsync(cancellationToken);

        db.CashMovements.Add(new CashMovement
        {
            CashAccountId = request.CashAccountId,
            SourceType = "CommissionPayment",
            CommissionPaymentId = payment.CourierCommissionPaymentId,
            Direction = "OUT",
            Amount = request.Amount,
            OccurredAt = now,
            RecordedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return payment.ToResponse();
    }
}
