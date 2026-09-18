USE [master];
GO
IF DB_ID(N'DeliveryDB') IS NULL
    EXEC(N'CREATE DATABASE [DeliveryDB];');
GO
USE [DeliveryDB];
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
GO

/* إعادة تشغيل آمنة أثناء التطوير: احذف العلاقات ثم الجداول ثم أنشئ من جديد. */
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.CashMovement', N'U') IS NOT NULL
    BEGIN
        ALTER TABLE [dbo].[CashMovement] DROP CONSTRAINT IF EXISTS [FK_CashMovement_CausedByAdjustment];
        ALTER TABLE [dbo].[Adjustment] DROP CONSTRAINT IF EXISTS [FK_Adjustment_CorrectsCashMovement];
    END;

    DROP TABLE IF EXISTS [dbo].[RemittanceAllocation];
    DROP TABLE IF EXISTS [dbo].[DeliveryAttempt];
    DROP TABLE IF EXISTS [dbo].[OrderEvent];
    DROP TABLE IF EXISTS [dbo].[MerchantPhone];
    DROP TABLE IF EXISTS [dbo].[CourierCommissionPayment];
    DROP TABLE IF EXISTS [dbo].[MerchantPayout];
    DROP TABLE IF EXISTS [dbo].[MerchantSettlementLine];
    DROP TABLE IF EXISTS [dbo].[CashMovement];
    DROP TABLE IF EXISTS [dbo].[Adjustment];
    DROP TABLE IF EXISTS [dbo].[Collection];
    DROP TABLE IF EXISTS [dbo].[CourierCommission];
    DROP TABLE IF EXISTS [dbo].[MerchantAccrual];
    DROP TABLE IF EXISTS [dbo].[CourierRemittance];
    DROP TABLE IF EXISTS [dbo].[MerchantSettlement];
    DROP TABLE IF EXISTS [dbo].[Expense];
    DROP TABLE IF EXISTS [dbo].[OrderAssignment];
    DROP TABLE IF EXISTS [dbo].[DeliveryOrder];
    DROP TABLE IF EXISTS [dbo].[AuditEvent];
    DROP TABLE IF EXISTS [dbo].[CashAccount];
    DROP TABLE IF EXISTS [dbo].[Merchant];
    DROP TABLE IF EXISTS [dbo].[Courier];
    DROP TABLE IF EXISTS [dbo].[CompanySettings];

    CREATE TABLE [dbo].[CompanySettings]
    (
        [CompanySettingsId] int NOT NULL,
        [CompanyName] nvarchar(150) NOT NULL,
        [CurrencyCode] char(3) NOT NULL,
        [TimeZoneId] nvarchar(100) NOT NULL,
        [OrderNumberPrefix] nvarchar(20) NOT NULL,
        [NextOrderNumber] bigint NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CompanySettings] PRIMARY KEY ([CompanySettingsId]),
        CONSTRAINT [CK_CompanySettings_Id] CHECK ([CompanySettingsId] = 1),
        CONSTRAINT [CK_CompanySettings_NextOrderNumber] CHECK ([NextOrderNumber] > 0),
        CONSTRAINT [CK_CompanySettings_Prefix] CHECK (LEN(LTRIM(RTRIM([OrderNumberPrefix]))) > 0)
    );

    CREATE TABLE [dbo].[Merchant]
    (
        [MerchantId] int IDENTITY(1, 1) NOT NULL,
        [MerchantName] nvarchar(150) NOT NULL,
        [DefaultMerchantFee] decimal(18, 3) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_Merchant_IsActive] DEFAULT (1),
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Merchant_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Merchant] PRIMARY KEY ([MerchantId]),
        CONSTRAINT [CK_Merchant_DefaultMerchantFee] CHECK ([DefaultMerchantFee] >= 0),
        CONSTRAINT [CK_Merchant_Name] CHECK (LEN(LTRIM(RTRIM([MerchantName]))) > 0)
    );

    CREATE TABLE [dbo].[Courier]
    (
        [CourierId] int IDENTITY(1, 1) NOT NULL,
        [CourierName] nvarchar(150) NOT NULL,
        [PhoneNumber] nvarchar(25) NOT NULL,
        [DefaultCommission] decimal(18, 3) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_Courier_IsActive] DEFAULT (1),
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Courier_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Courier] PRIMARY KEY ([CourierId]),
        CONSTRAINT [UQ_Courier_PhoneNumber] UNIQUE ([PhoneNumber]),
        CONSTRAINT [CK_Courier_DefaultCommission] CHECK ([DefaultCommission] >= 0),
        CONSTRAINT [CK_Courier_Name] CHECK (LEN(LTRIM(RTRIM([CourierName]))) > 0)
    );

    CREATE TABLE [dbo].[CashAccount]
    (
        [CashAccountId] int IDENTITY(1, 1) NOT NULL,
        [AccountName] nvarchar(100) NOT NULL,
        [AccountType] varchar(20) NOT NULL,
        [OpeningBalance] decimal(18, 3) NOT NULL,
        [OpenedAt] datetime2 NOT NULL CONSTRAINT [DF_CashAccount_OpenedAt] DEFAULT (SYSUTCDATETIME()),
        [IsActive] bit NOT NULL CONSTRAINT [DF_CashAccount_IsActive] DEFAULT (1),
        CONSTRAINT [PK_CashAccount] PRIMARY KEY ([CashAccountId]),
        CONSTRAINT [UQ_CashAccount_AccountName] UNIQUE ([AccountName]),
        CONSTRAINT [CK_CashAccount_AccountType] CHECK ([AccountType] IN ('CashBox', 'Bank')),
        CONSTRAINT [CK_CashAccount_OpeningBalance] CHECK ([OpeningBalance] >= 0)
    );

    CREATE TABLE [dbo].[DeliveryOrder]
    (
        [OrderId] bigint IDENTITY(1, 1) NOT NULL,
        [MerchantId] int NOT NULL,
        [InternalOrderNumber] nvarchar(40) NOT NULL,
        [ExternalReference] nvarchar(100) NULL,
        [RecipientName] nvarchar(150) NOT NULL,
        [RecipientPhoneNumber] nvarchar(25) NOT NULL,
        [DeliveryAddress] nvarchar(500) NOT NULL,
        [GoodsAmount] decimal(18, 3) NOT NULL,
        [RecipientFee] decimal(18, 3) NOT NULL,
        [MerchantFee] decimal(18, 3) NOT NULL,
        [CourierCommissionAmount] decimal(18, 3) NOT NULL,
        [Status] varchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_DeliveryOrder_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] datetime2 NOT NULL CONSTRAINT [DF_DeliveryOrder_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_DeliveryOrder] PRIMARY KEY ([OrderId]),
        CONSTRAINT [UQ_DeliveryOrder_InternalOrderNumber] UNIQUE ([InternalOrderNumber]),
        CONSTRAINT [CK_DeliveryOrder_InternalOrderNumber] CHECK (LEN(LTRIM(RTRIM([InternalOrderNumber]))) > 0),
        CONSTRAINT [CK_DeliveryOrder_ExternalReference] CHECK ([ExternalReference] IS NULL OR LEN(LTRIM(RTRIM([ExternalReference]))) > 0),
        CONSTRAINT [CK_DeliveryOrder_Amounts] CHECK (
            [GoodsAmount] >= 0
            AND [RecipientFee] >= 0
            AND [MerchantFee] >= 0
            AND [CourierCommissionAmount] >= 0
            AND [MerchantFee] <= [GoodsAmount]
            AND ([GoodsAmount] + [RecipientFee]) > 0
        ),
        CONSTRAINT [CK_DeliveryOrder_Status] CHECK ([Status] IN (
            'Created', 'Assigned', 'OutForDelivery', 'Delivered', 'Failed', 'Returned', 'Cancelled'
        )),
        CONSTRAINT [CK_DeliveryOrder_UpdatedAt] CHECK ([UpdatedAt] >= [CreatedAt])
    );

    CREATE TABLE [dbo].[MerchantPhone]
    (
        [MerchantPhoneId] int IDENTITY(1, 1) NOT NULL,
        [MerchantId] int NOT NULL,
        [PhoneNumber] nvarchar(25) NOT NULL,
        [IsPrimary] bit NOT NULL CONSTRAINT [DF_MerchantPhone_IsPrimary] DEFAULT (0),
        CONSTRAINT [PK_MerchantPhone] PRIMARY KEY ([MerchantPhoneId]),
        CONSTRAINT [UQ_MerchantPhone_Merchant_Phone] UNIQUE ([MerchantId], [PhoneNumber])
    );

    CREATE TABLE [dbo].[OrderAssignment]
    (
        [OrderAssignmentId] bigint IDENTITY(1, 1) NOT NULL,
        [OrderId] bigint NOT NULL,
        [CourierId] int NOT NULL,
        [StartedAt] datetime2 NOT NULL CONSTRAINT [DF_OrderAssignment_StartedAt] DEFAULT (SYSUTCDATETIME()),
        [EndedAt] datetime2 NULL,
        [AssignedBy] nvarchar(100) NOT NULL,
        [ChangeReason] nvarchar(500) NULL,
        [HandoverProofReference] nvarchar(500) NULL,
        CONSTRAINT [PK_OrderAssignment] PRIMARY KEY ([OrderAssignmentId]),
        CONSTRAINT [CK_OrderAssignment_EndedAt] CHECK ([EndedAt] IS NULL OR [EndedAt] >= [StartedAt])
    );

    CREATE TABLE [dbo].[OrderEvent]
    (
        [OrderEventId] bigint IDENTITY(1, 1) NOT NULL,
        [OrderId] bigint NOT NULL,
        [PreviousStatus] varchar(30) NULL,
        [NewStatus] varchar(30) NOT NULL,
        [OccurredAt] datetime2 NOT NULL,
        [RecordedAt] datetime2 NOT NULL CONSTRAINT [DF_OrderEvent_RecordedAt] DEFAULT (SYSUTCDATETIME()),
        [RecordedBy] nvarchar(100) NOT NULL,
        [Reason] nvarchar(500) NULL,
        CONSTRAINT [PK_OrderEvent] PRIMARY KEY ([OrderEventId]),
        CONSTRAINT [CK_OrderEvent_NewStatus] CHECK ([NewStatus] IN (
            'Created', 'Assigned', 'OutForDelivery', 'Delivered', 'Failed', 'Returned', 'Cancelled'
        )),
        CONSTRAINT [CK_OrderEvent_PreviousStatus] CHECK ([PreviousStatus] IS NULL OR [PreviousStatus] IN (
            'Created', 'Assigned', 'OutForDelivery', 'Delivered', 'Failed', 'Returned', 'Cancelled'
        )),
        CONSTRAINT [CK_OrderEvent_Times] CHECK ([RecordedAt] >= [OccurredAt])
    );

    CREATE TABLE [dbo].[DeliveryAttempt]
    (
        [DeliveryAttemptId] bigint IDENTITY(1, 1) NOT NULL,
        [OrderAssignmentId] bigint NOT NULL,
        [Result] varchar(30) NOT NULL,
        [FailureReason] nvarchar(500) NULL,
        [AttemptedAt] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        CONSTRAINT [PK_DeliveryAttempt] PRIMARY KEY ([DeliveryAttemptId]),
        CONSTRAINT [CK_DeliveryAttempt_Result] CHECK ([Result] IN ('Delivered', 'Failed', 'Rescheduled'))
    );

    CREATE TABLE [dbo].[Collection]
    (
        [CollectionId] bigint IDENTITY(1, 1) NOT NULL,
        [OrderId] bigint NOT NULL,
        [CourierId] int NOT NULL,
        [IdempotencyKey] uniqueidentifier NOT NULL,
        [Amount] decimal(18, 3) NOT NULL,
        [CollectedAt] datetime2 NOT NULL,
        [RecordedBy] nvarchar(100) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Collection] PRIMARY KEY ([CollectionId]),
        CONSTRAINT [UQ_Collection_OrderId] UNIQUE ([OrderId]),
        CONSTRAINT [UQ_Collection_IdempotencyKey] UNIQUE ([IdempotencyKey]),
        CONSTRAINT [CK_Collection_Amount] CHECK ([Amount] > 0)
    );

    CREATE TABLE [dbo].[CourierCommission]
    (
        [CourierCommissionId] bigint IDENTITY(1, 1) NOT NULL,
        [OrderId] bigint NOT NULL,
        [CourierId] int NOT NULL,
        [Amount] decimal(18, 3) NOT NULL,
        [AccruedAt] datetime2 NOT NULL CONSTRAINT [DF_CourierCommission_AccruedAt] DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CourierCommission] PRIMARY KEY ([CourierCommissionId]),
        CONSTRAINT [UQ_CourierCommission_OrderId] UNIQUE ([OrderId]),
        CONSTRAINT [CK_CourierCommission_Amount] CHECK ([Amount] >= 0)
    );

    CREATE TABLE [dbo].[MerchantAccrual]
    (
        [MerchantAccrualId] bigint IDENTITY(1, 1) NOT NULL,
        [OrderId] bigint NOT NULL,
        [MerchantAmount] decimal(18, 3) NOT NULL,
        [CompanyFee] decimal(18, 3) NOT NULL,
        [AccruedAt] datetime2 NOT NULL CONSTRAINT [DF_MerchantAccrual_AccruedAt] DEFAULT (SYSUTCDATETIME()),
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_MerchantAccrual] PRIMARY KEY ([MerchantAccrualId]),
        CONSTRAINT [UQ_MerchantAccrual_OrderId] UNIQUE ([OrderId]),
        CONSTRAINT [CK_MerchantAccrual_Amounts] CHECK ([MerchantAmount] >= 0 AND [CompanyFee] >= 0)
    );

    CREATE TABLE [dbo].[CourierRemittance]
    (
        [CourierRemittanceId] bigint IDENTITY(1, 1) NOT NULL,
        [CourierId] int NOT NULL,
        [CashAccountId] int NOT NULL,
        [IdempotencyKey] uniqueidentifier NOT NULL,
        [Amount] decimal(18, 3) NOT NULL,
        [ReceivedBy] nvarchar(100) NOT NULL,
        [RemittedAt] datetime2 NOT NULL,
        [ProofReference] nvarchar(500) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CourierRemittance] PRIMARY KEY ([CourierRemittanceId]),
        CONSTRAINT [UQ_CourierRemittance_IdempotencyKey] UNIQUE ([IdempotencyKey]),
        CONSTRAINT [CK_CourierRemittance_Amount] CHECK ([Amount] > 0)
    );

    CREATE TABLE [dbo].[RemittanceAllocation]
    (
        [RemittanceAllocationId] bigint IDENTITY(1, 1) NOT NULL,
        [CourierRemittanceId] bigint NOT NULL,
        [CollectionId] bigint NOT NULL,
        [AllocatedAmount] decimal(18, 3) NOT NULL,
        [AllocatedAt] datetime2 NOT NULL CONSTRAINT [DF_RemittanceAllocation_AllocatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_RemittanceAllocation] PRIMARY KEY ([RemittanceAllocationId]),
        CONSTRAINT [UQ_RemittanceAllocation_Remittance_Collection] UNIQUE ([CourierRemittanceId], [CollectionId]),
        CONSTRAINT [CK_RemittanceAllocation_Amount] CHECK ([AllocatedAmount] > 0)
    );

    CREATE TABLE [dbo].[CourierCommissionPayment]
    (
        [CourierCommissionPaymentId] bigint IDENTITY(1, 1) NOT NULL,
        [CourierCommissionId] bigint NOT NULL,
        [IdempotencyKey] uniqueidentifier NOT NULL,
        [PaymentReference] nvarchar(100) NOT NULL,
        [Amount] decimal(18, 3) NOT NULL,
        [PaymentMethod] varchar(20) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        [PaidBy] nvarchar(100) NOT NULL,
        [ProofReference] nvarchar(500) NULL,
        CONSTRAINT [PK_CourierCommissionPayment] PRIMARY KEY ([CourierCommissionPaymentId]),
        CONSTRAINT [UQ_CourierCommissionPayment_IdempotencyKey] UNIQUE ([IdempotencyKey]),
        CONSTRAINT [UQ_CourierCommissionPayment_PaymentReference] UNIQUE ([PaymentReference]),
        CONSTRAINT [CK_CourierCommissionPayment_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [CK_CourierCommissionPayment_Method] CHECK ([PaymentMethod] IN ('Cash', 'BankTransfer'))
    );

    CREATE TABLE [dbo].[MerchantSettlement]
    (
        [MerchantSettlementId] bigint IDENTITY(1, 1) NOT NULL,
        [MerchantId] int NOT NULL,
        [Status] varchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_MerchantSettlement_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [CreatedBy] nvarchar(100) NOT NULL,
        [ApprovedAt] datetime2 NULL,
        [ApprovedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_MerchantSettlement] PRIMARY KEY ([MerchantSettlementId]),
        CONSTRAINT [CK_MerchantSettlement_Status] CHECK ([Status] IN ('Draft', 'Approved', 'Cancelled'))
    );

    CREATE TABLE [dbo].[MerchantSettlementLine]
    (
        [MerchantSettlementLineId] bigint IDENTITY(1, 1) NOT NULL,
        [MerchantSettlementId] bigint NOT NULL,
        [MerchantAccrualId] bigint NOT NULL,
        [SettlementAmount] decimal(18, 3) NOT NULL,
        [ReleasedAt] datetime2 NULL,
        CONSTRAINT [PK_MerchantSettlementLine] PRIMARY KEY ([MerchantSettlementLineId]),
        CONSTRAINT [CK_MerchantSettlementLine_Amount] CHECK ([SettlementAmount] > 0)
    );

    CREATE TABLE [dbo].[MerchantPayout]
    (
        [MerchantPayoutId] bigint IDENTITY(1, 1) NOT NULL,
        [MerchantSettlementId] bigint NOT NULL,
        [IdempotencyKey] uniqueidentifier NOT NULL,
        [PaymentReference] nvarchar(100) NOT NULL,
        [Amount] decimal(18, 3) NOT NULL,
        [PaymentMethod] varchar(20) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        [PaidBy] nvarchar(100) NOT NULL,
        [ProofReference] nvarchar(500) NULL,
        CONSTRAINT [PK_MerchantPayout] PRIMARY KEY ([MerchantPayoutId]),
        CONSTRAINT [UQ_MerchantPayout_IdempotencyKey] UNIQUE ([IdempotencyKey]),
        CONSTRAINT [UQ_MerchantPayout_PaymentReference] UNIQUE ([PaymentReference]),
        CONSTRAINT [CK_MerchantPayout_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [CK_MerchantPayout_Method] CHECK ([PaymentMethod] IN ('Cash', 'BankTransfer'))
    );

    CREATE TABLE [dbo].[Expense]
    (
        [ExpenseId] bigint IDENTITY(1, 1) NOT NULL,
        [IdempotencyKey] uniqueidentifier NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Amount] decimal(18, 3) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [PaidAt] datetime2 NOT NULL,
        [RecordedBy] nvarchar(100) NOT NULL,
        [ApprovedBy] nvarchar(100) NOT NULL,
        [ProofReference] nvarchar(500) NULL,
        CONSTRAINT [PK_Expense] PRIMARY KEY ([ExpenseId]),
        CONSTRAINT [UQ_Expense_IdempotencyKey] UNIQUE ([IdempotencyKey]),
        CONSTRAINT [CK_Expense_Amount] CHECK ([Amount] > 0)
    );

    CREATE TABLE [dbo].[Adjustment]
    (
        [AdjustmentId] bigint IDENTITY(1, 1) NOT NULL,
        [TargetType] varchar(30) NOT NULL,
        [OriginalCollectionId] bigint NULL,
        [OriginalAccrualId] bigint NULL,
        [OriginalCommissionId] bigint NULL,
        [OriginalCashMovementId] bigint NULL,
        [ReversesAdjustmentId] bigint NULL,
        [IdempotencyKey] uniqueidentifier NOT NULL,
        [AmountDelta] decimal(18, 3) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] varchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_Adjustment_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [CreatedBy] nvarchar(100) NOT NULL,
        [ApprovedAt] datetime2 NULL,
        [ApprovedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Adjustment] PRIMARY KEY ([AdjustmentId]),
        CONSTRAINT [UQ_Adjustment_IdempotencyKey] UNIQUE ([IdempotencyKey]),
        CONSTRAINT [CK_Adjustment_AmountDelta] CHECK ([AmountDelta] <> 0),
        CONSTRAINT [CK_Adjustment_Status] CHECK ([Status] IN ('Draft', 'Approved', 'Reversed')),
        CONSTRAINT [CK_Adjustment_NoSelfReverse] CHECK (
            [ReversesAdjustmentId] IS NULL OR [ReversesAdjustmentId] <> [AdjustmentId]
        ),
        CONSTRAINT [CK_Adjustment_ExclusiveTarget] CHECK (
            (
                [TargetType] = 'Collection'
                AND [OriginalCollectionId] IS NOT NULL
                AND [OriginalAccrualId] IS NULL
                AND [OriginalCommissionId] IS NULL
                AND [OriginalCashMovementId] IS NULL
            )
            OR (
                [TargetType] = 'MerchantAccrual'
                AND [OriginalAccrualId] IS NOT NULL
                AND [OriginalCollectionId] IS NULL
                AND [OriginalCommissionId] IS NULL
                AND [OriginalCashMovementId] IS NULL
            )
            OR (
                [TargetType] = 'CourierCommission'
                AND [OriginalCommissionId] IS NOT NULL
                AND [OriginalCollectionId] IS NULL
                AND [OriginalAccrualId] IS NULL
                AND [OriginalCashMovementId] IS NULL
            )
            OR (
                [TargetType] = 'CashMovement'
                AND [OriginalCashMovementId] IS NOT NULL
                AND [OriginalCollectionId] IS NULL
                AND [OriginalAccrualId] IS NULL
                AND [OriginalCommissionId] IS NULL
            )
        )
    );

    CREATE TABLE [dbo].[CashMovement]
    (
        [CashMovementId] bigint IDENTITY(1, 1) NOT NULL,
        [CashAccountId] int NOT NULL,
        [SourceType] varchar(30) NOT NULL,
        [CourierRemittanceId] bigint NULL,
        [MerchantPayoutId] bigint NULL,
        [CommissionPaymentId] bigint NULL,
        [ExpenseId] bigint NULL,
        [AdjustmentId] bigint NULL,
        [Direction] varchar(3) NOT NULL,
        [Amount] decimal(18, 3) NOT NULL,
        [OccurredAt] datetime2 NOT NULL,
        [RecordedAt] datetime2 NOT NULL CONSTRAINT [DF_CashMovement_RecordedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_CashMovement] PRIMARY KEY ([CashMovementId]),
        CONSTRAINT [CK_CashMovement_Amount] CHECK ([Amount] > 0),
        CONSTRAINT [CK_CashMovement_Direction] CHECK ([Direction] IN ('IN', 'OUT')),
        CONSTRAINT [CK_CashMovement_Times] CHECK ([RecordedAt] >= [OccurredAt]),
        CONSTRAINT [CK_CashMovement_ExclusiveSource] CHECK (
            (
                [SourceType] = 'CourierRemittance'
                AND [CourierRemittanceId] IS NOT NULL
                AND [MerchantPayoutId] IS NULL
                AND [CommissionPaymentId] IS NULL
                AND [ExpenseId] IS NULL
                AND [AdjustmentId] IS NULL
                AND [Direction] = 'IN'
            )
            OR (
                [SourceType] = 'MerchantPayout'
                AND [MerchantPayoutId] IS NOT NULL
                AND [CourierRemittanceId] IS NULL
                AND [CommissionPaymentId] IS NULL
                AND [ExpenseId] IS NULL
                AND [AdjustmentId] IS NULL
                AND [Direction] = 'OUT'
            )
            OR (
                [SourceType] = 'CommissionPayment'
                AND [CommissionPaymentId] IS NOT NULL
                AND [CourierRemittanceId] IS NULL
                AND [MerchantPayoutId] IS NULL
                AND [ExpenseId] IS NULL
                AND [AdjustmentId] IS NULL
                AND [Direction] = 'OUT'
            )
            OR (
                [SourceType] = 'Expense'
                AND [ExpenseId] IS NOT NULL
                AND [CourierRemittanceId] IS NULL
                AND [MerchantPayoutId] IS NULL
                AND [CommissionPaymentId] IS NULL
                AND [AdjustmentId] IS NULL
                AND [Direction] = 'OUT'
            )
            OR (
                [SourceType] = 'Adjustment'
                AND [AdjustmentId] IS NOT NULL
                AND [CourierRemittanceId] IS NULL
                AND [MerchantPayoutId] IS NULL
                AND [CommissionPaymentId] IS NULL
                AND [ExpenseId] IS NULL
            )
        )
    );

    CREATE TABLE [dbo].[AuditEvent]
    (
        [AuditEventId] bigint IDENTITY(1, 1) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [Action] varchar(30) NOT NULL,
        [BeforeData] nvarchar(max) NULL,
        [AfterData] nvarchar(max) NULL,
        [OccurredAt] datetime2 NOT NULL CONSTRAINT [DF_AuditEvent_OccurredAt] DEFAULT (SYSUTCDATETIME()),
        [PerformedBy] nvarchar(100) NOT NULL,
        [CorrelationId] uniqueidentifier NULL,
        CONSTRAINT [PK_AuditEvent] PRIMARY KEY ([AuditEventId]),
        CONSTRAINT [CK_AuditEvent_Action] CHECK ([Action] IN (
            'Insert', 'Update', 'StatusChange', 'Approve', 'Cancel', 'Reverse'
        ))
    );

    ALTER TABLE [dbo].[DeliveryOrder] WITH CHECK
        ADD CONSTRAINT [FK_DeliveryOrder_MerchantId]
        FOREIGN KEY ([MerchantId]) REFERENCES [dbo].[Merchant] ([MerchantId]);

    ALTER TABLE [dbo].[MerchantPhone] WITH CHECK
        ADD CONSTRAINT [FK_MerchantPhone_MerchantId]
        FOREIGN KEY ([MerchantId]) REFERENCES [dbo].[Merchant] ([MerchantId]);

    ALTER TABLE [dbo].[OrderAssignment] WITH CHECK
        ADD CONSTRAINT [FK_OrderAssignment_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[DeliveryOrder] ([OrderId]);

    

    ALTER TABLE [dbo].[OrderEvent] WITH CHECK
        ADD CONSTRAINT [FK_OrderEvent_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[DeliveryOrder] ([OrderId]);

    ALTER TABLE [dbo].[DeliveryAttempt] WITH CHECK
        ADD CONSTRAINT [FK_DeliveryAttempt_OrderAssignmentId]
        FOREIGN KEY ([OrderAssignmentId]) REFERENCES [dbo].[OrderAssignment] ([OrderAssignmentId]);

    ALTER TABLE [dbo].[Collection] WITH CHECK
        ADD CONSTRAINT [FK_Collection_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[DeliveryOrder] ([OrderId]);

    ALTER TABLE [dbo].[Collection] WITH CHECK
        ADD CONSTRAINT [FK_Collection_CourierId]
        FOREIGN KEY ([CourierId]) REFERENCES [dbo].[Courier] ([CourierId]);

    ALTER TABLE [dbo].[CourierCommission] WITH CHECK
        ADD CONSTRAINT [FK_CourierCommission_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[DeliveryOrder] ([OrderId]);

    ALTER TABLE [dbo].[CourierCommission] WITH CHECK
        ADD CONSTRAINT [FK_CourierCommission_CourierId]
        FOREIGN KEY ([CourierId]) REFERENCES [dbo].[Courier] ([CourierId]);

    ALTER TABLE [dbo].[OrderAssignment] WITH CHECK
        ADD CONSTRAINT [FK_OrderAssignment_CourierId]
        FOREIGN KEY ([CourierId]) REFERENCES [dbo].[Courier] ([CourierId]);

    ALTER TABLE [dbo].[MerchantAccrual] WITH CHECK
        ADD CONSTRAINT [FK_MerchantAccrual_OrderId]
        FOREIGN KEY ([OrderId]) REFERENCES [dbo].[DeliveryOrder] ([OrderId]);

    ALTER TABLE [dbo].[CourierRemittance] WITH CHECK
        ADD CONSTRAINT [FK_CourierRemittance_CourierId]
        FOREIGN KEY ([CourierId]) REFERENCES [dbo].[Courier] ([CourierId]);

    ALTER TABLE [dbo].[CourierRemittance] WITH CHECK
        ADD CONSTRAINT [FK_CourierRemittance_CashAccountId]
        FOREIGN KEY ([CashAccountId]) REFERENCES [dbo].[CashAccount] ([CashAccountId]);

    ALTER TABLE [dbo].[RemittanceAllocation] WITH CHECK
        ADD CONSTRAINT [FK_RemittanceAllocation_CourierRemittanceId]
        FOREIGN KEY ([CourierRemittanceId]) REFERENCES [dbo].[CourierRemittance] ([CourierRemittanceId]);

    ALTER TABLE [dbo].[RemittanceAllocation] WITH CHECK
        ADD CONSTRAINT [FK_RemittanceAllocation_CollectionId]
        FOREIGN KEY ([CollectionId]) REFERENCES [dbo].[Collection] ([CollectionId]);

    ALTER TABLE [dbo].[CourierCommissionPayment] WITH CHECK
        ADD CONSTRAINT [FK_CourierCommissionPayment_CourierCommissionId]
        FOREIGN KEY ([CourierCommissionId]) REFERENCES [dbo].[CourierCommission] ([CourierCommissionId]);

    ALTER TABLE [dbo].[MerchantSettlement] WITH CHECK
        ADD CONSTRAINT [FK_MerchantSettlement_MerchantId]
        FOREIGN KEY ([MerchantId]) REFERENCES [dbo].[Merchant] ([MerchantId]);

    ALTER TABLE [dbo].[MerchantSettlementLine] WITH CHECK
        ADD CONSTRAINT [FK_MerchantSettlementLine_MerchantSettlementId]
        FOREIGN KEY ([MerchantSettlementId]) REFERENCES [dbo].[MerchantSettlement] ([MerchantSettlementId]);

    ALTER TABLE [dbo].[MerchantSettlementLine] WITH CHECK
        ADD CONSTRAINT [FK_MerchantSettlementLine_MerchantAccrualId]
        FOREIGN KEY ([MerchantAccrualId]) REFERENCES [dbo].[MerchantAccrual] ([MerchantAccrualId]);

    ALTER TABLE [dbo].[MerchantPayout] WITH CHECK
        ADD CONSTRAINT [FK_MerchantPayout_MerchantSettlementId]
        FOREIGN KEY ([MerchantSettlementId]) REFERENCES [dbo].[MerchantSettlement] ([MerchantSettlementId]);

    ALTER TABLE [dbo].[CashMovement] WITH CHECK
        ADD CONSTRAINT [FK_CashMovement_CashAccountId]
        FOREIGN KEY ([CashAccountId]) REFERENCES [dbo].[CashAccount] ([CashAccountId]);

    ALTER TABLE [dbo].[CashMovement] WITH CHECK
        ADD CONSTRAINT [FK_CashMovement_CourierRemittanceId]
        FOREIGN KEY ([CourierRemittanceId]) REFERENCES [dbo].[CourierRemittance] ([CourierRemittanceId]);

    ALTER TABLE [dbo].[CashMovement] WITH CHECK
        ADD CONSTRAINT [FK_CashMovement_MerchantPayoutId]
        FOREIGN KEY ([MerchantPayoutId]) REFERENCES [dbo].[MerchantPayout] ([MerchantPayoutId]);

    ALTER TABLE [dbo].[CashMovement] WITH CHECK
        ADD CONSTRAINT [FK_CashMovement_CommissionPaymentId]
        FOREIGN KEY ([CommissionPaymentId]) REFERENCES [dbo].[CourierCommissionPayment] ([CourierCommissionPaymentId]);

    ALTER TABLE [dbo].[CashMovement] WITH CHECK
        ADD CONSTRAINT [FK_CashMovement_ExpenseId]
        FOREIGN KEY ([ExpenseId]) REFERENCES [dbo].[Expense] ([ExpenseId]);

    /* حركة ناتجة عن تصحيح (دور مختلف عن تصحيح حركة قديمة). */
    ALTER TABLE [dbo].[CashMovement] WITH CHECK
        ADD CONSTRAINT [FK_CashMovement_CausedByAdjustment]
        FOREIGN KEY ([AdjustmentId]) REFERENCES [dbo].[Adjustment] ([AdjustmentId]);

    ALTER TABLE [dbo].[Adjustment] WITH CHECK
        ADD CONSTRAINT [FK_Adjustment_OriginalCollectionId]
        FOREIGN KEY ([OriginalCollectionId]) REFERENCES [dbo].[Collection] ([CollectionId]);

    ALTER TABLE [dbo].[Adjustment] WITH CHECK
        ADD CONSTRAINT [FK_Adjustment_OriginalAccrualId]
        FOREIGN KEY ([OriginalAccrualId]) REFERENCES [dbo].[MerchantAccrual] ([MerchantAccrualId]);

    ALTER TABLE [dbo].[Adjustment] WITH CHECK
        ADD CONSTRAINT [FK_Adjustment_OriginalCommissionId]
        FOREIGN KEY ([OriginalCommissionId]) REFERENCES [dbo].[CourierCommission] ([CourierCommissionId]);

    /* تصحيح يصوّب حركة صندوق موجودة مسبقاً. */
    ALTER TABLE [dbo].[Adjustment] WITH CHECK
        ADD CONSTRAINT [FK_Adjustment_CorrectsCashMovement]
        FOREIGN KEY ([OriginalCashMovementId]) REFERENCES [dbo].[CashMovement] ([CashMovementId]);

    ALTER TABLE [dbo].[Adjustment] WITH CHECK
        ADD CONSTRAINT [FK_Adjustment_ReversesAdjustmentId]
        FOREIGN KEY ([ReversesAdjustmentId]) REFERENCES [dbo].[Adjustment] ([AdjustmentId]);

    CREATE UNIQUE INDEX [UQ_DeliveryOrder_MerchantExternal]
        ON [dbo].[DeliveryOrder] ([MerchantId], [ExternalReference])
        WHERE [ExternalReference] IS NOT NULL;

    CREATE UNIQUE INDEX [UQ_MerchantPhone_Primary]
        ON [dbo].[MerchantPhone] ([MerchantId])
        WHERE [IsPrimary] = 1;

    CREATE UNIQUE INDEX [UQ_OrderAssignment_Open]
        ON [dbo].[OrderAssignment] ([OrderId])
        WHERE [EndedAt] IS NULL;

    CREATE UNIQUE INDEX [UQ_MerchantSettlementLine_OpenAccrual]
        ON [dbo].[MerchantSettlementLine] ([MerchantAccrualId])
        WHERE [ReleasedAt] IS NULL;

    CREATE UNIQUE INDEX [UQ_CashMovement_CourierRemittanceId]
        ON [dbo].[CashMovement] ([CourierRemittanceId])
        WHERE [CourierRemittanceId] IS NOT NULL;

    CREATE UNIQUE INDEX [UQ_CashMovement_MerchantPayoutId]
        ON [dbo].[CashMovement] ([MerchantPayoutId])
        WHERE [MerchantPayoutId] IS NOT NULL;

    CREATE UNIQUE INDEX [UQ_CashMovement_CommissionPaymentId]
        ON [dbo].[CashMovement] ([CommissionPaymentId])
        WHERE [CommissionPaymentId] IS NOT NULL;

    CREATE UNIQUE INDEX [UQ_CashMovement_ExpenseId]
        ON [dbo].[CashMovement] ([ExpenseId])
        WHERE [ExpenseId] IS NOT NULL;

    CREATE UNIQUE INDEX [UQ_CashMovement_AdjustmentId]
        ON [dbo].[CashMovement] ([AdjustmentId])
        WHERE [AdjustmentId] IS NOT NULL;

    CREATE INDEX [IX_DeliveryOrder_MerchantId] ON [dbo].[DeliveryOrder] ([MerchantId]);
    CREATE INDEX [IX_DeliveryOrder_Status_CreatedAt] ON [dbo].[DeliveryOrder] ([Status], [CreatedAt]);
    CREATE INDEX [IX_OrderAssignment_CourierId] ON [dbo].[OrderAssignment] ([CourierId]);
    CREATE INDEX [IX_OrderEvent_OrderId_OccurredAt] ON [dbo].[OrderEvent] ([OrderId], [OccurredAt]);
    CREATE INDEX [IX_DeliveryAttempt_OrderAssignmentId] ON [dbo].[DeliveryAttempt] ([OrderAssignmentId]);
    CREATE INDEX [IX_Collection_CourierId] ON [dbo].[Collection] ([CourierId]);
    CREATE INDEX [IX_CourierRemittance_CourierId] ON [dbo].[CourierRemittance] ([CourierId]);
    CREATE INDEX [IX_RemittanceAllocation_CollectionId] ON [dbo].[RemittanceAllocation] ([CollectionId]);
    CREATE INDEX [IX_MerchantSettlement_MerchantId] ON [dbo].[MerchantSettlement] ([MerchantId]);
    CREATE INDEX [IX_MerchantPayout_MerchantSettlementId] ON [dbo].[MerchantPayout] ([MerchantSettlementId]);
    CREATE INDEX [IX_CashMovement_CashAccountId_OccurredAt] ON [dbo].[CashMovement] ([CashAccountId], [OccurredAt]);
    CREATE INDEX [IX_AuditEvent_Entity] ON [dbo].[AuditEvent] ([EntityType], [EntityId], [OccurredAt]);

    IF NOT EXISTS (SELECT 1 FROM [dbo].[CompanySettings] WHERE [CompanySettingsId] = 1)
    BEGIN
        INSERT INTO [dbo].[CompanySettings]
        (
            [CompanySettingsId],
            [CompanyName],
            [CurrencyCode],
            [TimeZoneId],
            [OrderNumberPrefix],
            [NextOrderNumber]
        )
        VALUES
        (1, N'Delivery Company', 'JOD', N'Asia/Amman', N'DLV', 1);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
