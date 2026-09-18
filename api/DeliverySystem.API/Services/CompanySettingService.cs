using DeliverySystem.API.Data;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Mappings;
using DeliverySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Services;

public sealed class CompanySettingService(DeliveryDbContext db)
{
    public async Task<CompanySettingResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var settings = await db.CompanySettings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.CompanySettingsId == 1, cancellationToken);

        if (settings is null)
        {
            throw new NotFoundException("Company settings were not found.");
        }

        return settings.ToResponse();
    }

    public async Task<CompanySettingResponse> UpdateAsync(
        UpdateCompanySettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.CompanySettings.FirstOrDefaultAsync(
            item => item.CompanySettingsId == 1,
            cancellationToken);

        if (settings is null)
        {
            throw new NotFoundException("Company settings were not found.");
        }

        settings.CompanyName = request.CompanyName.Trim();
        settings.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        settings.TimeZoneId = request.TimeZoneId.Trim();
        settings.OrderNumberPrefix = request.OrderNumberPrefix.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return settings.ToResponse();
    }
}
