using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class CourierService(DeliveryDbContext db)
{
    public async Task<List<CourierResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var couriers = await db.Couriers
            .AsNoTracking()
            .OrderBy(courier => courier.CourierId)
            .ToListAsync(cancellationToken);

        return couriers.Select(courier => courier.ToResponse()).ToList();
    }

    public async Task<CourierResponse> CreateAsync(
        CreateCourierRequest request,
        CancellationToken cancellationToken = default)
    {
        var phone = request.PhoneNumber.Trim();
        var exists = await db.Couriers.AnyAsync(courier => courier.PhoneNumber == phone, cancellationToken);
        if (exists)
        {
            throw new ConflictException("A courier with this phone number already exists.");
        }

        var courier = new Courier
        {
            CourierName = request.CourierName.Trim(),
            PhoneNumber = phone,
            DefaultCommission = request.DefaultCommission,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        db.Couriers.Add(courier);
        await db.SaveChangesAsync(cancellationToken);
        return courier.ToResponse();
    }
}
