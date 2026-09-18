using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class ExpenseService(DeliveryDbContext db)
{
    public async Task<List<ExpenseResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var expenses = await db.Expenses
            .AsNoTracking()
            .OrderBy(item => item.ExpenseId)
            .ToListAsync(cancellationToken);

        return expenses.Select(item => item.ToResponse()).ToList();
    }

    public async Task<ExpenseResponse> CreateAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

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

        var duplicateKey = await db.Expenses.AnyAsync(
            item => item.IdempotencyKey == request.IdempotencyKey,
            cancellationToken);

        if (duplicateKey)
        {
            throw new ConflictException("This expense was already recorded.");
        }

        var now = DateTime.UtcNow;
        var expense = new Expense
        {
            IdempotencyKey = request.IdempotencyKey,
            Category = request.Category.Trim(),
            Amount = request.Amount,
            Reason = request.Reason.Trim(),
            PaidAt = now,
            RecordedBy = request.RecordedBy.Trim(),
            ApprovedBy = request.ApprovedBy.Trim(),
            ProofReference = string.IsNullOrWhiteSpace(request.ProofReference)
                ? null
                : request.ProofReference.Trim()
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync(cancellationToken);

        db.CashMovements.Add(new CashMovement
        {
            CashAccountId = request.CashAccountId,
            SourceType = "Expense",
            ExpenseId = expense.ExpenseId,
            Direction = "OUT",
            Amount = request.Amount,
            OccurredAt = now,
            RecordedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return expense.ToResponse();
    }
}
