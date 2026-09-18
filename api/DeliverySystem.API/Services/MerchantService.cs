using DeliverySystem.API.Data;
using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class MerchantService(DeliveryDbContext db)
{
    public async Task<List<MerchantResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var merchants = await db.Merchants
            .AsNoTracking()
            .OrderBy(merchant => merchant.MerchantId)
            .ToListAsync(cancellationToken);

        return merchants.Select(merchant => merchant.ToResponse()).ToList();
    }

    public async Task<MerchantResponse> CreateAsync(
        CreateMerchantRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = new Merchant
        {
            MerchantName = request.MerchantName.Trim(),
            DefaultMerchantFee = request.DefaultMerchantFee,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        db.Merchants.Add(merchant);
        await db.SaveChangesAsync(cancellationToken);
        return merchant.ToResponse();
    }
}
