using DeliverySystem.API.Data.Entities;
using DeliverySystem.API.Models;

namespace DeliverySystem.API.Mappings;

public static class ResponseMappings
{
    public static MerchantResponse ToResponse(this Merchant merchant) =>
        new(
            merchant.MerchantId,
            merchant.MerchantName,
            merchant.DefaultMerchantFee,
            merchant.IsActive,
            merchant.CreatedAt);

    public static CourierResponse ToResponse(this Courier courier) =>
        new(
            courier.CourierId,
            courier.CourierName,
            courier.PhoneNumber,
            courier.DefaultCommission,
            courier.IsActive,
            courier.CreatedAt);

    public static DeliveryOrderResponse ToResponse(this DeliveryOrder order) =>
        new(
            order.OrderId,
            order.MerchantId,
            order.InternalOrderNumber,
            order.ExternalReference,
            order.RecipientName,
            order.RecipientPhoneNumber,
            order.DeliveryAddress,
            order.GoodsAmount,
            order.RecipientFee,
            order.MerchantFee,
            order.CourierCommissionAmount,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt);

    public static OrderAssignmentResponse ToResponse(this OrderAssignment assignment) =>
        new(
            assignment.OrderAssignmentId,
            assignment.OrderId,
            assignment.CourierId,
            assignment.StartedAt,
            assignment.EndedAt,
            assignment.AssignedBy,
            assignment.ChangeReason,
            assignment.HandoverProofReference);

    public static DeliveryAttemptResponse ToResponse(this DeliveryAttempt attempt) =>
        new(
            attempt.DeliveryAttemptId,
            attempt.OrderAssignmentId,
            attempt.Result,
            attempt.FailureReason,
            attempt.AttemptedAt,
            attempt.Notes);

    public static CollectionResponse ToResponse(this Collection collection) =>
        new(
            collection.CollectionId,
            collection.OrderId,
            collection.CourierId,
            collection.IdempotencyKey,
            collection.Amount,
            collection.CollectedAt,
            collection.RecordedBy);

    public static CourierRemittanceResponse ToResponse(this CourierRemittance remittance) =>
        new(
            remittance.CourierRemittanceId,
            remittance.CourierId,
            remittance.CashAccountId,
            remittance.IdempotencyKey,
            remittance.Amount,
            remittance.ReceivedBy,
            remittance.RemittedAt,
            remittance.ProofReference);

       public static CourierCommissionPaymentResponse ToResponse(this CourierCommissionPayment payment) =>
        new(
            payment.CourierCommissionPaymentId,
            payment.CourierCommissionId,
            payment.IdempotencyKey,
            payment.PaymentReference,
            payment.Amount,
            payment.PaymentMethod,
            payment.PaidAt,
            payment.PaidBy,
            payment.ProofReference);

    public static MerchantSettlementResponse ToResponse(this MerchantSettlement settlement) =>
        new(
            settlement.MerchantSettlementId,
            settlement.MerchantId,
            settlement.Status,
            settlement.CreatedAt,
            settlement.CreatedBy,
            settlement.ApprovedAt,
            settlement.ApprovedBy);

    public static MerchantPayoutResponse ToResponse(this MerchantPayout payout) =>
        new(
            payout.MerchantPayoutId,
            payout.MerchantSettlementId,
            payout.IdempotencyKey,
            payout.PaymentReference,
            payout.Amount,
            payout.PaymentMethod,
            payout.PaidAt,
            payout.PaidBy,
            payout.ProofReference);

    public static ExpenseResponse ToResponse(this Expense expense) =>
        new(
            expense.ExpenseId,
            expense.IdempotencyKey,
            expense.Category,
            expense.Amount,
            expense.Reason,
            expense.PaidAt,
            expense.RecordedBy,
            expense.ApprovedBy,
            expense.ProofReference);

    public static AdjustmentResponse ToResponse(this Adjustment adjustment) =>
        new(
            adjustment.AdjustmentId,
            adjustment.TargetType,
            adjustment.OriginalCollectionId,
            adjustment.OriginalAccrualId,
            adjustment.OriginalCommissionId,
            adjustment.OriginalCashMovementId,
            adjustment.ReversesAdjustmentId,
            adjustment.IdempotencyKey,
            adjustment.AmountDelta,
            adjustment.Reason,
            adjustment.Status,
            adjustment.CreatedAt,
            adjustment.CreatedBy,
            adjustment.ApprovedAt,
            adjustment.ApprovedBy);

    public static MerchantPhoneResponse ToResponse(this MerchantPhone phone) =>
        new(
            phone.MerchantPhoneId,
            phone.MerchantId,
            phone.PhoneNumber,
            phone.IsPrimary);

    public static CompanySettingResponse ToResponse(this CompanySetting settings) =>
        new(
            settings.CompanySettingsId,
            settings.CompanyName,
            settings.CurrencyCode,
            settings.TimeZoneId,
            settings.OrderNumberPrefix,
            settings.NextOrderNumber);
}