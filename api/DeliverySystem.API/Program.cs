using DeliverySystem.API.Data;
using DeliverySystem.API.Errors;
using DeliverySystem.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDbContext<DeliveryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DeliveryDB")));
builder.Services.AddScoped<MerchantService>();
builder.Services.AddScoped<CourierService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<DeliveryAttemptService>();
builder.Services.AddScoped<CollectionService>();
builder.Services.AddScoped<CourierRemittanceService>();
builder.Services.AddScoped<CourierCommissionPaymentService>();
builder.Services.AddScoped<MerchantSettlementService>();
builder.Services.AddScoped<MerchantPayoutService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<AdjustmentService>();
builder.Services.AddScoped<MerchantPhoneService>();
builder.Services.AddScoped<CompanySettingService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
