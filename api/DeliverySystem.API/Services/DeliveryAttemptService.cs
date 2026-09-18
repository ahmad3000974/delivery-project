using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Domain;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class DeliveryAttemptService(DeliveryDbContext db)
{
    public async Task<List<DeliveryAttemptResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var attempts = await db.DeliveryAttempts
            .AsNoTracking()
            .OrderBy(attempt => attempt.DeliveryAttemptId)
            .ToListAsync(cancellationToken);

        return attempts.Select(attempt => attempt.ToResponse()).ToList();
    }

    public async Task<DeliveryAttemptResponse> CreateAsync(
        CreateDeliveryAttemptRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = request.Result.Trim();
        var failureReason = string.IsNullOrWhiteSpace(request.FailureReason)
            ? null
            : request.FailureReason.Trim();
        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var assignment = await db.OrderAssignments
            .Include(item => item.Order)
            .FirstOrDefaultAsync(
                item => item.OrderAssignmentId == request.OrderAssignmentId,
                cancellationToken);

        if (assignment is null)
        {
            throw new NotFoundException("Assignment was not found.");
        }

        if (assignment.EndedAt is not null)
        {
            throw new BusinessRuleException("Assignment is already closed.");
        }

        var order = assignment.Order;
        if (order.Status != OrderStatuses.OutForDelivery)
        {
            throw new BusinessRuleException(
                "An attempt can be recorded only while the order is OutForDelivery.");
        }

        var now = DateTime.UtcNow;
        var attempt = new DeliveryAttempt
        {
            OrderAssignmentId = assignment.OrderAssignmentId,
            Result = result,
            FailureReason = failureReason,
            Notes = notes,
            AttemptedAt = now
        };

        db.DeliveryAttempts.Add(attempt);

        var newStatus = result switch
        {
            "Delivered" => OrderStatuses.Delivered,
            "Failed" => OrderStatuses.Failed,
            _ => null
        };

        if (newStatus is not null)
        {
            if (!OrderStatuses.CanTransition(order.Status, newStatus))
            {
                throw new BusinessRuleException(
                    $"Order status cannot change from '{order.Status}' to '{newStatus}'.");
            }

            var previousStatus = order.Status;
            order.Status = newStatus;
            order.UpdatedAt = now;
            db.OrderEvents.Add(new OrderEvent
            {
                Order = order,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                OccurredAt = now,
                RecordedAt = now,
                RecordedBy = "api",
                Reason = failureReason ?? notes
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return attempt.ToResponse();
    }
}
