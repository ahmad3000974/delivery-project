using System;
using System.Collections.Generic;
using DeliverySystem.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.API.Data;

public partial class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adjustment> Adjustments { get; set; }

    public virtual DbSet<AuditEvent> AuditEvents { get; set; }

    public virtual DbSet<CashAccount> CashAccounts { get; set; }

    public virtual DbSet<CashMovement> CashMovements { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<CompanySetting> CompanySettings { get; set; }

    public virtual DbSet<Courier> Couriers { get; set; }

    public virtual DbSet<CourierCommission> CourierCommissions { get; set; }

    public virtual DbSet<CourierCommissionPayment> CourierCommissionPayments { get; set; }

    public virtual DbSet<CourierRemittance> CourierRemittances { get; set; }

    public virtual DbSet<DeliveryAttempt> DeliveryAttempts { get; set; }

    public virtual DbSet<DeliveryOrder> DeliveryOrders { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<MerchantAccrual> MerchantAccruals { get; set; }

    public virtual DbSet<MerchantPayout> MerchantPayouts { get; set; }

    public virtual DbSet<MerchantPhone> MerchantPhones { get; set; }

    public virtual DbSet<MerchantSettlement> MerchantSettlements { get; set; }

    public virtual DbSet<MerchantSettlementLine> MerchantSettlementLines { get; set; }

    public virtual DbSet<OrderAssignment> OrderAssignments { get; set; }

    public virtual DbSet<OrderEvent> OrderEvents { get; set; }

    public virtual DbSet<RemittanceAllocation> RemittanceAllocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adjustment>(entity =>
        {
            entity.ToTable("Adjustment");

            entity.HasIndex(e => e.IdempotencyKey, "UQ_Adjustment_IdempotencyKey").IsUnique();

            entity.Property(e => e.AmountDelta).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TargetType)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.OriginalAccrual).WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.OriginalAccrualId)
                .HasConstraintName("FK_Adjustment_OriginalAccrualId");

            entity.HasOne(d => d.OriginalCashMovement).WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.OriginalCashMovementId)
                .HasConstraintName("FK_Adjustment_CorrectsCashMovement");

            entity.HasOne(d => d.OriginalCollection).WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.OriginalCollectionId)
                .HasConstraintName("FK_Adjustment_OriginalCollectionId");

            entity.HasOne(d => d.OriginalCommission).WithMany(p => p.Adjustments)
                .HasForeignKey(d => d.OriginalCommissionId)
                .HasConstraintName("FK_Adjustment_OriginalCommissionId");

            entity.HasOne(d => d.ReversesAdjustment).WithMany(p => p.InverseReversesAdjustment)
                .HasForeignKey(d => d.ReversesAdjustmentId)
                .HasConstraintName("FK_Adjustment_ReversesAdjustmentId");
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("AuditEvent");

            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.OccurredAt }, "IX_AuditEvent_Entity");

            entity.Property(e => e.Action)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.OccurredAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PerformedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<CashAccount>(entity =>
        {
            entity.ToTable("CashAccount");

            entity.HasIndex(e => e.AccountName, "UQ_CashAccount_AccountName").IsUnique();

            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.AccountType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OpenedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.OpeningBalance).HasColumnType("decimal(18, 3)");
        });

        modelBuilder.Entity<CashMovement>(entity =>
        {
            entity.ToTable("CashMovement");

            entity.HasIndex(e => new { e.CashAccountId, e.OccurredAt }, "IX_CashMovement_CashAccountId_OccurredAt");

            entity.HasIndex(e => e.AdjustmentId, "UQ_CashMovement_AdjustmentId")
                .IsUnique()
                .HasFilter("([AdjustmentId] IS NOT NULL)");

            entity.HasIndex(e => e.CommissionPaymentId, "UQ_CashMovement_CommissionPaymentId")
                .IsUnique()
                .HasFilter("([CommissionPaymentId] IS NOT NULL)");

            entity.HasIndex(e => e.CourierRemittanceId, "UQ_CashMovement_CourierRemittanceId")
                .IsUnique()
                .HasFilter("([CourierRemittanceId] IS NOT NULL)");

            entity.HasIndex(e => e.ExpenseId, "UQ_CashMovement_ExpenseId")
                .IsUnique()
                .HasFilter("([ExpenseId] IS NOT NULL)");

            entity.HasIndex(e => e.MerchantPayoutId, "UQ_CashMovement_MerchantPayoutId")
                .IsUnique()
                .HasFilter("([MerchantPayoutId] IS NOT NULL)");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Direction)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.SourceType)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Adjustment).WithOne(p => p.CashMovement)
                .HasForeignKey<CashMovement>(d => d.AdjustmentId)
                .HasConstraintName("FK_CashMovement_CausedByAdjustment");

            entity.HasOne(d => d.CashAccount).WithMany(p => p.CashMovements)
                .HasForeignKey(d => d.CashAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashMovement_CashAccountId");

            entity.HasOne(d => d.CommissionPayment).WithOne(p => p.CashMovement)
                .HasForeignKey<CashMovement>(d => d.CommissionPaymentId)
                .HasConstraintName("FK_CashMovement_CommissionPaymentId");

            entity.HasOne(d => d.CourierRemittance).WithOne(p => p.CashMovement)
                .HasForeignKey<CashMovement>(d => d.CourierRemittanceId)
                .HasConstraintName("FK_CashMovement_CourierRemittanceId");

            entity.HasOne(d => d.Expense).WithOne(p => p.CashMovement)
                .HasForeignKey<CashMovement>(d => d.ExpenseId)
                .HasConstraintName("FK_CashMovement_ExpenseId");

            entity.HasOne(d => d.MerchantPayout).WithOne(p => p.CashMovement)
                .HasForeignKey<CashMovement>(d => d.MerchantPayoutId)
                .HasConstraintName("FK_CashMovement_MerchantPayoutId");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.ToTable("Collection");

            entity.HasIndex(e => e.CourierId, "IX_Collection_CourierId");

            entity.HasIndex(e => e.IdempotencyKey, "UQ_Collection_IdempotencyKey").IsUnique();

            entity.HasIndex(e => e.OrderId, "UQ_Collection_OrderId").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.RecordedBy).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Courier).WithMany(p => p.Collections)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Collection_CourierId");

            entity.HasOne(d => d.Order).WithOne(p => p.Collection)
                .HasForeignKey<Collection>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Collection_OrderId");
        });

        modelBuilder.Entity<CompanySetting>(entity =>
        {
            entity.HasKey(e => e.CompanySettingsId);

            entity.ToTable("CompanySettings");

            entity.Property(e => e.CompanySettingsId).ValueGeneratedNever();
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.OrderNumberPrefix).HasMaxLength(20);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TimeZoneId).HasMaxLength(100);
        });

        modelBuilder.Entity<Courier>(entity =>
        {
            entity.ToTable("Courier");

            entity.HasIndex(e => e.PhoneNumber, "UQ_Courier_PhoneNumber").IsUnique();

            entity.Property(e => e.CourierName).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DefaultCommission).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PhoneNumber).HasMaxLength(25);
        });

        modelBuilder.Entity<CourierCommission>(entity =>
        {
            entity.ToTable("CourierCommission");

            entity.HasIndex(e => e.OrderId, "UQ_CourierCommission_OrderId").IsUnique();

            entity.Property(e => e.AccruedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Order).WithOne(p => p.CourierCommission)
                .HasForeignKey<CourierCommission>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourierCommission_OrderId");

            entity.HasOne(d => d.Courier).WithMany(p => p.CourierCommissions)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourierCommission_CourierId");
        });

        modelBuilder.Entity<CourierCommissionPayment>(entity =>
        {
            entity.ToTable("CourierCommissionPayment");

            entity.HasIndex(e => e.IdempotencyKey, "UQ_CourierCommissionPayment_IdempotencyKey").IsUnique();

            entity.HasIndex(e => e.PaymentReference, "UQ_CourierCommissionPayment_PaymentReference").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.PaidBy).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.ProofReference).HasMaxLength(500);

            entity.HasOne(d => d.CourierCommission).WithMany(p => p.CourierCommissionPayments)
                .HasForeignKey(d => d.CourierCommissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourierCommissionPayment_CourierCommissionId");
        });

        modelBuilder.Entity<CourierRemittance>(entity =>
        {
            entity.ToTable("CourierRemittance");

            entity.HasIndex(e => e.CourierId, "IX_CourierRemittance_CourierId");

            entity.HasIndex(e => e.IdempotencyKey, "UQ_CourierRemittance_IdempotencyKey").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ProofReference).HasMaxLength(500);
            entity.Property(e => e.ReceivedBy).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.CashAccount).WithMany(p => p.CourierRemittances)
                .HasForeignKey(d => d.CashAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourierRemittance_CashAccountId");

            entity.HasOne(d => d.Courier).WithMany(p => p.CourierRemittances)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourierRemittance_CourierId");
        });

        modelBuilder.Entity<DeliveryAttempt>(entity =>
        {
            entity.ToTable("DeliveryAttempt");

            entity.HasIndex(e => e.OrderAssignmentId, "IX_DeliveryAttempt_OrderAssignmentId");

            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Result)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.OrderAssignment).WithMany(p => p.DeliveryAttempts)
                .HasForeignKey(d => d.OrderAssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeliveryAttempt_OrderAssignmentId");
        });

        modelBuilder.Entity<DeliveryOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId);

            entity.ToTable("DeliveryOrder");

            entity.HasIndex(e => e.MerchantId, "IX_DeliveryOrder_MerchantId");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "IX_DeliveryOrder_Status_CreatedAt");

            entity.HasIndex(e => e.InternalOrderNumber, "UQ_DeliveryOrder_InternalOrderNumber").IsUnique();

            entity.HasIndex(e => new { e.MerchantId, e.ExternalReference }, "UQ_DeliveryOrder_MerchantExternal")
                .IsUnique()
                .HasFilter("([ExternalReference] IS NOT NULL)");

            entity.Property(e => e.CourierCommissionAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
            entity.Property(e => e.ExternalReference).HasMaxLength(100);
            entity.Property(e => e.GoodsAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.InternalOrderNumber).HasMaxLength(40);
            entity.Property(e => e.MerchantFee).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.RecipientFee).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.RecipientName).HasMaxLength(150);
            entity.Property(e => e.RecipientPhoneNumber).HasMaxLength(25);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Merchant).WithMany(p => p.DeliveryOrders)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeliveryOrder_MerchantId");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable("Expense");

            entity.HasIndex(e => e.IdempotencyKey, "UQ_Expense_IdempotencyKey").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.ProofReference).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RecordedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.ToTable("Merchant");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DefaultMerchantFee).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MerchantName).HasMaxLength(150);
        });

        modelBuilder.Entity<MerchantAccrual>(entity =>
        {
            entity.ToTable("MerchantAccrual");

            entity.HasIndex(e => e.OrderId, "UQ_MerchantAccrual_OrderId").IsUnique();

            entity.Property(e => e.AccruedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CompanyFee).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.MerchantAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Order).WithOne(p => p.MerchantAccrual)
                .HasForeignKey<MerchantAccrual>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantAccrual_OrderId");
        });

        modelBuilder.Entity<MerchantPayout>(entity =>
        {
            entity.ToTable("MerchantPayout");

            entity.HasIndex(e => e.MerchantSettlementId, "IX_MerchantPayout_MerchantSettlementId");

            entity.HasIndex(e => e.IdempotencyKey, "UQ_MerchantPayout_IdempotencyKey").IsUnique();

            entity.HasIndex(e => e.PaymentReference, "UQ_MerchantPayout_PaymentReference").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.PaidBy).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.Property(e => e.ProofReference).HasMaxLength(500);

            entity.HasOne(d => d.MerchantSettlement).WithMany(p => p.MerchantPayouts)
                .HasForeignKey(d => d.MerchantSettlementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantPayout_MerchantSettlementId");
        });

        modelBuilder.Entity<MerchantPhone>(entity =>
        {
            entity.ToTable("MerchantPhone");

            entity.HasIndex(e => new { e.MerchantId, e.PhoneNumber }, "UQ_MerchantPhone_Merchant_Phone").IsUnique();

            entity.HasIndex(e => e.MerchantId, "UQ_MerchantPhone_Primary")
                .IsUnique()
                .HasFilter("([IsPrimary]=(1))");

            entity.Property(e => e.PhoneNumber).HasMaxLength(25);

            entity.HasOne(d => d.Merchant).WithMany(p => p.MerchantPhones)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantPhone_MerchantId");
        });

        modelBuilder.Entity<MerchantSettlement>(entity =>
        {
            entity.ToTable("MerchantSettlement");

            entity.HasIndex(e => e.MerchantId, "IX_MerchantSettlement_MerchantId");

            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Merchant).WithMany(p => p.MerchantSettlements)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantSettlement_MerchantId");
        });

        modelBuilder.Entity<MerchantSettlementLine>(entity =>
        {
            entity.ToTable("MerchantSettlementLine");

            entity.HasIndex(e => e.MerchantAccrualId, "UQ_MerchantSettlementLine_OpenAccrual")
                .IsUnique()
                .HasFilter("([ReleasedAt] IS NULL)");

            entity.Property(e => e.SettlementAmount).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.MerchantAccrual).WithOne(p => p.MerchantSettlementLine)
                .HasForeignKey<MerchantSettlementLine>(d => d.MerchantAccrualId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantSettlementLine_MerchantAccrualId");

            entity.HasOne(d => d.MerchantSettlement).WithMany(p => p.MerchantSettlementLines)
                .HasForeignKey(d => d.MerchantSettlementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MerchantSettlementLine_MerchantSettlementId");
        });

        modelBuilder.Entity<OrderAssignment>(entity =>
        {
            entity.ToTable("OrderAssignment");

            entity.HasIndex(e => e.CourierId, "IX_OrderAssignment_CourierId");

            entity.HasIndex(e => e.OrderId, "UQ_OrderAssignment_Open")
                .IsUnique()
                .HasFilter("([EndedAt] IS NULL)");

            entity.Property(e => e.AssignedBy).HasMaxLength(100);
            entity.Property(e => e.ChangeReason).HasMaxLength(500);
            entity.Property(e => e.HandoverProofReference).HasMaxLength(500);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Courier).WithMany(p => p.OrderAssignments)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderAssignment_CourierId");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderAssignments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderAssignment_OrderId");
        });

        modelBuilder.Entity<OrderEvent>(entity =>
        {
            entity.ToTable("OrderEvent");

            entity.HasIndex(e => new { e.OrderId, e.OccurredAt }, "IX_OrderEvent_OrderId_OccurredAt");

            entity.Property(e => e.NewStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.PreviousStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RecordedBy).HasMaxLength(100);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderEvents)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderEvent_OrderId");
        });

        modelBuilder.Entity<RemittanceAllocation>(entity =>
        {
            entity.ToTable("RemittanceAllocation");

            entity.HasIndex(e => e.CollectionId, "IX_RemittanceAllocation_CollectionId");

            entity.HasIndex(e => new { e.CourierRemittanceId, e.CollectionId }, "UQ_RemittanceAllocation_Remittance_Collection").IsUnique();

            entity.Property(e => e.AllocatedAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.AllocatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Collection).WithMany(p => p.RemittanceAllocations)
                .HasForeignKey(d => d.CollectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RemittanceAllocation_CollectionId");

            entity.HasOne(d => d.CourierRemittance).WithMany(p => p.RemittanceAllocations)
                .HasForeignKey(d => d.CourierRemittanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RemittanceAllocation_CourierRemittanceId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
