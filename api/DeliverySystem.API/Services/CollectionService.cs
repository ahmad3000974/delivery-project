using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Domain;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class CollectionService
{
    private readonly DeliveryDbContext _db;

    public CollectionService(DeliveryDbContext db)
    {
        _db = db;
    }

    public async Task<List<CollectionResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var collections = await _db.Collections
            .AsNoTracking()
            .OrderBy(collection => collection.CollectionId)
            .ToListAsync(cancellationToken);

        return collections.Select(collection => collection.ToResponse()).ToList();
    }

    public async Task<CollectionResponse> CreateAsync(
        CreateCollectionRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var order = await _db.DeliveryOrders.FirstOrDefaultAsync(
            item => item.OrderId == request.OrderId,
            cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("Order was not found.");
        }

        if (order.Status != OrderStatuses.Delivered)
        {
            throw new BusinessRuleException("Order is not delivered.");
        }

        var hasOpenAssignment = await _db.OrderAssignments.AnyAsync(
            item => item.OrderId == request.OrderId
                && item.CourierId == request.CourierId
                && item.EndedAt == null,
            cancellationToken);

        if (!hasOpenAssignment)
        {
            throw new BusinessRuleException("Courier has no open assignment on this order.");
        }

        var alreadyCollected = await _db.Collections.AnyAsync(
            item => item.OrderId == request.OrderId,
            cancellationToken);

        if (alreadyCollected)
        {
            throw new ConflictException("This order was already collected.");
        }

        var collection = new Collection
        {
            OrderId = request.OrderId,
            CourierId = request.CourierId,
            Amount = request.Amount,
            IdempotencyKey = request.IdempotencyKey,
            RecordedBy = request.RecordedBy.Trim(),
            CollectedAt = DateTime.UtcNow
        };

        var now = collection.CollectedAt;

        _db.Collections.Add(collection);
        _db.CourierCommissions.Add(new CourierCommission
        {
            OrderId = order.OrderId,
            CourierId = request.CourierId,
            Amount = order.CourierCommissionAmount,
            AccruedAt = now
        });
        _db.MerchantAccruals.Add(new MerchantAccrual
        {
            OrderId = order.OrderId,
            MerchantAmount = order.GoodsAmount - order.MerchantFee,
            CompanyFee = order.MerchantFee,
            AccruedAt = now
        });
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return collection.ToResponse();
    }
}
