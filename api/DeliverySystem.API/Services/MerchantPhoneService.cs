using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class MerchantPhoneService(DeliveryDbContext db)
{
    public async Task<List<MerchantPhoneResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var phones = await db.MerchantPhones
            .AsNoTracking()
            .OrderBy(item => item.MerchantPhoneId)
            .ToListAsync(cancellationToken);

        return phones.Select(item => item.ToResponse()).ToList();
    }

    public async Task<MerchantPhoneResponse> CreateAsync(
        CreateMerchantPhoneRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await db.Merchants.FirstOrDefaultAsync(
            item => item.MerchantId == request.MerchantId,
            cancellationToken);

        if (merchant is null)
        {
            throw new NotFoundException("Merchant was not found.");
        }

        var phoneNumber = request.PhoneNumber.Trim();
        var duplicate = await db.MerchantPhones.AnyAsync(
            item => item.MerchantId == request.MerchantId && item.PhoneNumber == phoneNumber,
            cancellationToken);

        if (duplicate)
        {
            throw new ConflictException("This phone number already exists for the merchant.");
        }

        if (request.IsPrimary)
        {
            var currentPrimary = await db.MerchantPhones
                .Where(item => item.MerchantId == request.MerchantId && item.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var phone in currentPrimary)
            {
                phone.IsPrimary = false;
            }
        }

        var merchantPhone = new MerchantPhone
        {
            MerchantId = request.MerchantId,
            PhoneNumber = phoneNumber,
            IsPrimary = request.IsPrimary
        };

        db.MerchantPhones.Add(merchantPhone);
        await db.SaveChangesAsync(cancellationToken);
        return merchantPhone.ToResponse();
    }
}
