USE master;

USE TcmbSimulator;

-- ============================================================
-- Drop existing objects
-- ============================================================

DROP PROCEDURE IF EXISTS sp_PaymentOrders_Accept;
GO

DROP TABLE IF EXISTS PaymentStatusHistory;
DROP TABLE IF EXISTS PaymentOrders;
DROP TABLE IF EXISTS ParticipantBanks;
GO

-- ============================================================
-- MAIN TABLES
-- ============================================================

CREATE TABLE ParticipantBanks (
    BankCode char(5) NOT NULL PRIMARY KEY,
    BankName nvarchar(150) NOT NULL,
    ApiBaseUrl nvarchar(500) NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAtUtc datetime2 NOT NULL DEFAULT sysutcdatetime()
);
GO

CREATE TABLE PaymentOrders (
    PaymentOrderId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CentralReference varchar(64) NOT NULL,
    SenderBankCode char(5) NOT NULL REFERENCES ParticipantBanks(BankCode),
    SenderReference varchar(64) NOT NULL,
    RequestHash char(64) NOT NULL,
    ReceiverBankCode char(5) NOT NULL REFERENCES ParticipantBanks(BankCode),
    ReceiverIban varchar(34) NOT NULL,
    ReceiverName nvarchar(200) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode char(3) NOT NULL,
    Description nvarchar(255) NULL,
    Status nvarchar(30) NOT NULL,
    ReceivedAtUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    RoutedAtUtc datetime2 NULL,
    CompletedAtUtc datetime2 NULL,
    FailureReason nvarchar(500) NULL,
    CONSTRAINT CK_PaymentOrders_Amount_Positive CHECK (Amount > 0),
    CONSTRAINT CK_PaymentOrders_Status CHECK (Status IN (
        'Accepted','Routing','Completed','Rejected','PendingReconciliation')),
    CONSTRAINT UQ_PaymentOrders_SenderReference
        UNIQUE(SenderBankCode,SenderReference),
    CONSTRAINT UQ_PaymentOrders_CentralReference
        UNIQUE(CentralReference)
);
CREATE INDEX IX_PaymentOrders_Status_ReceivedAtUtc
    ON PaymentOrders(Status,ReceivedAtUtc);
CREATE INDEX IX_PaymentOrders_ReceiverBank_Status
    ON PaymentOrders(ReceiverBankCode,Status);
GO

CREATE TABLE PaymentStatusHistory (
    PaymentStatusHistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PaymentOrderId int NOT NULL REFERENCES PaymentOrders(PaymentOrderId),
    Status nvarchar(30) NOT NULL,
    ChangedAtUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    Reason nvarchar(500) NULL
);
CREATE INDEX IX_PaymentStatusHistory_Order_ChangedAtUtc
    ON PaymentStatusHistory(PaymentOrderId,ChangedAtUtc);
GO

-- ============================================================
-- SIMULATED PARTICIPANTS
-- ============================================================

INSERT INTO ParticipantBanks(BankCode,BankName,ApiBaseUrl)
VALUES
    ('00001','BankApp','http://localhost:5000'),
    ('00002','BankApp2','http://localhost:5006');
GO

-- ============================================================
-- SPs — PAYMENT ORDERS
-- ============================================================

CREATE OR ALTER PROCEDURE sp_PaymentOrders_Accept
    @SenderBankCode char(5),
    @SenderReference varchar(64),
    @RequestHash char(64),
    @ReceiverBankCode char(5),
    @ReceiverIban varchar(34),
    @ReceiverName nvarchar(200),
    @Amount decimal(18,2),
    @CurrencyCode char(3),
    @Description nvarchar(255) = NULL,
    @CentralReference varchar(64)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ExistingRequestHash char(64);

        SELECT @ExistingRequestHash = RequestHash
        FROM PaymentOrders WITH (UPDLOCK, HOLDLOCK)
        WHERE SenderBankCode = @SenderBankCode
          AND SenderReference = @SenderReference;

        IF @ExistingRequestHash IS NOT NULL
        BEGIN
            IF @ExistingRequestHash <> @RequestHash
                THROW 50000, 'The sender reference was already used with different payment data.', 1;

            SELECT CentralReference,
                   Status,
                   ReceivedAtUtc AS AcceptedAtUtc
            FROM PaymentOrders
            WHERE SenderBankCode = @SenderBankCode
              AND SenderReference = @SenderReference;

            COMMIT TRANSACTION;
            RETURN;
        END

        IF @Amount <= 0
            THROW 50000, 'Amount must be greater than zero.', 1;

        IF @CurrencyCode <> 'TRY'
            THROW 50000, 'Only TRY payments are supported.', 1;

        IF @SenderBankCode = @ReceiverBankCode
            THROW 50000, 'The sender and receiver banks must be different.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM ParticipantBanks
            WHERE BankCode = @SenderBankCode
              AND IsActive = 1
        )
            THROW 50000, 'Sender bank is not an active participant.', 1;

        IF NOT EXISTS (
            SELECT 1
            FROM ParticipantBanks
            WHERE BankCode = @ReceiverBankCode
              AND IsActive = 1
        )
            THROW 50000, 'Receiver bank is not an active participant.', 1;

        INSERT INTO PaymentOrders (
            CentralReference,
            SenderBankCode,
            SenderReference,
            RequestHash,
            ReceiverBankCode,
            ReceiverIban,
            ReceiverName,
            Amount,
            CurrencyCode,
            Description,
            Status
        )
        VALUES (
            @CentralReference,
            @SenderBankCode,
            @SenderReference,
            @RequestHash,
            @ReceiverBankCode,
            @ReceiverIban,
            @ReceiverName,
            @Amount,
            @CurrencyCode,
            @Description,
            'Accepted'
        );

        DECLARE @PaymentOrderId int = SCOPE_IDENTITY();

        INSERT INTO PaymentStatusHistory (PaymentOrderId, Status)
        VALUES (@PaymentOrderId, 'Accepted');

        SELECT CentralReference,
               Status,
               ReceivedAtUtc AS AcceptedAtUtc
        FROM PaymentOrders
        WHERE PaymentOrderId = @PaymentOrderId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO
