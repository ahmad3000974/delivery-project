using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Domain;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class OrderService(DeliveryDbContext db)
{
    public async Task<List<DeliveryOrderResponse>> ListOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await db.DeliveryOrders
            .AsNoTracking()
            .OrderBy(order => order.OrderId)
            .ToListAsync(cancellationToken);

        return orders.Select(order => order.ToResponse()).ToList();
    }

    public async Task<DeliveryOrderResponse> CreateOrderAsync(
        CreateDeliveryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await db.Merchants.FirstOrDefaultAsync(
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

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var internalNumber = string.IsNullOrWhiteSpace(request.InternalOrderNumber)
            ? await NextInternalOrderNumberAsync(cancellationToken)
            : request.InternalOrderNumber.Trim();

        var now = DateTime.UtcNow;
        var order = new DeliveryOrder
        {
            MerchantId = request.MerchantId,
            InternalOrderNumber = internalNumber,
            ExternalReference = string.IsNullOrWhiteSpace(request.ExternalReference)
                ? null
                : request.ExternalReference.Trim(),
            RecipientName = request.RecipientName.Trim(),
            RecipientPhoneNumber = request.RecipientPhoneNumber.Trim(),
            DeliveryAddress = request.DeliveryAddress.Trim(),
            GoodsAmount = request.GoodsAmount,
            RecipientFee = request.RecipientFee,
            MerchantFee = request.MerchantFee,
            CourierCommissionAmount = request.CourierCommissionAmount,
            Status = OrderStatuses.Created,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.DeliveryOrders.Add(order);
        db.OrderEvents.Add(new OrderEvent
        {
            Order = order,
            PreviousStatus = null,
            NewStatus = OrderStatuses.Created,
            OccurredAt = now,
            RecordedAt = now,
            RecordedBy = "api"
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return order.ToResponse();
    }

    public async Task<List<OrderAssignmentResponse>> ListAssignmentsAsync(
        CancellationToken cancellationToken = default)
    {
        var assignments = await db.OrderAssignments
            .AsNoTracking()
            .OrderBy(assignment => assignment.OrderAssignmentId)
            .ToListAsync(cancellationToken);

        return assignments.Select(assignment => assignment.ToResponse()).ToList();
    }

    public async Task<OrderAssignmentResponse> AssignAsync(
        CreateOrderAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var order = await db.DeliveryOrders.FirstOrDefaultAsync(
            item => item.OrderId == request.OrderId,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order was not found.");
        }

        if (!OrderStatuses.CanAssign(order.Status))
        {
            throw new BusinessRuleException($"Order status '{order.Status}' cannot be assigned.");
        }

        var hasOpenAssignment = await db.OrderAssignments.AnyAsync(
            item => item.OrderId == request.OrderId && item.EndedAt == null,
            cancellationToken);

        if (hasOpenAssignment)
        {
            throw new ConflictException("This order already has an open assignment.");
        }

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

        var now = DateTime.UtcNow;
        var assignment = new OrderAssignment
        {
            OrderId = request.OrderId,
            CourierId = request.CourierId,
            AssignedBy = request.AssignedBy.Trim(),
            ChangeReason = request.ChangeReason,
            HandoverProofReference = request.HandoverProofReference,
            StartedAt = now,
            EndedAt = null
        };

        var previousStatus = order.Status;
        order.Status = OrderStatuses.Assigned;
        order.UpdatedAt = now;
        db.OrderAssignments.Add(assignment);
        db.OrderEvents.Add(new OrderEvent
        {
            Order = order,
            PreviousStatus = previousStatus,
            NewStatus = OrderStatuses.Assigned,
            OccurredAt = now,
            RecordedAt = now,
            RecordedBy = assignment.AssignedBy,
            Reason = request.ChangeReason
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return assignment.ToResponse();
    }

    public async Task<DeliveryOrderResponse> UpdateOrderStatusAsync(
        long id,
        UpdateDeliveryOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var newStatus = request.Status.Trim();
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

        if (!OrderStatuses.IsKnown(newStatus))
        {
            throw new BusinessRuleException($"Status '{newStatus}' is not allowed.");
        }

        if (newStatus == OrderStatuses.Assigned)
        {
            throw new BusinessRuleException("Assign the order through the assignments API.");
        }

        if (OrderStatuses.RequiresReason(newStatus) && reason is null)
        {
            throw new BusinessRuleException($"Status '{newStatus}' requires a reason.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var order = await db.DeliveryOrders.FirstOrDefaultAsync(
            item => item.OrderId == id,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order was not found.");
        }

        if (!OrderStatuses.CanTransition(order.Status, newStatus))
        {
            throw new BusinessRuleException(
                $"Order status cannot change from '{order.Status}' to '{newStatus}'.");
        }

        if (OrderStatuses.RequiresOpenAssignment(newStatus))
        {
            var hasOpenAssignment = await db.OrderAssignments.AnyAsync(
                item => item.OrderId == id && item.EndedAt == null,
                cancellationToken);

            if (!hasOpenAssignment)
            {
                throw new BusinessRuleException("Order has no open assignment.");
            }
        }

        if (newStatus == OrderStatuses.Cancelled)
        {
            var openAssignments = await db.OrderAssignments
                .Where(item => item.OrderId == id && item.EndedAt == null)
                .ToListAsync(cancellationToken);

            var endedAt = DateTime.UtcNow;
            foreach (var assignment in openAssignments)
            {
                assignment.EndedAt = endedAt;
            }
        }

        var now = DateTime.UtcNow;
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
            Reason = reason
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return order.ToResponse();
    }

    private async Task<string> NextInternalOrderNumberAsync(CancellationToken cancellationToken)
    {
        var settings = await db.CompanySettings.FirstOrDefaultAsync(cancellationToken)
            ?? throw new BusinessRuleException("Company settings are missing.");

        var number = $"{settings.OrderNumberPrefix}{settings.NextOrderNumber}";
        settings.NextOrderNumber += 1;
        return number;
    }
}
