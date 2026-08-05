USE master;

USE TcmbSimulator;

-- ============================================================
-- Drop existing objects
-- ============================================================

DROP PROCEDURE IF EXISTS sp_RoutingOutbox_MarkFailed;
DROP PROCEDURE IF EXISTS sp_RoutingOutbox_MarkResult;
DROP PROCEDURE IF EXISTS sp_RoutingOutbox_MarkRouting;
DROP PROCEDURE IF EXISTS sp_RoutingOutbox_Pending;
DROP PROCEDURE IF EXISTS sp_PaymentOrders_Accept;
GO

DROP TABLE IF EXISTS RoutingOutboxMessages;
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

CREATE TABLE RoutingOutboxMessages (
    RoutingOutboxMessageId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PaymentOrderId int NOT NULL REFERENCES PaymentOrders(PaymentOrderId),
    MessageType nvarchar(100) NOT NULL,
    CreatedAtUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    ProcessedAtUtc datetime2 NULL,
    AttemptCount int NOT NULL DEFAULT 0,
    LastError nvarchar(1000) NULL,
    CONSTRAINT UQ_RoutingOutboxMessages_Order_Message
        UNIQUE(PaymentOrderId,MessageType)
);
CREATE INDEX IX_RoutingOutboxMessages_Pending
    ON RoutingOutboxMessages(ProcessedAtUtc,CreatedAtUtc);
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
        DECLARE @ExistingPaymentOrderId int;
        DECLARE @ExistingStatus nvarchar(30);

        SELECT @ExistingRequestHash = RequestHash,
               @ExistingPaymentOrderId = PaymentOrderId,
               @ExistingStatus = Status
        FROM PaymentOrders WITH (UPDLOCK, HOLDLOCK)
        WHERE SenderBankCode = @SenderBankCode
          AND SenderReference = @SenderReference;

        IF @ExistingRequestHash IS NOT NULL
        BEGIN
            IF @ExistingRequestHash <> @RequestHash
                THROW 50000, 'The sender reference was already used with different payment data.', 1;

            IF @ExistingStatus = 'Accepted'
               AND NOT EXISTS (
                   SELECT 1
                   FROM RoutingOutboxMessages
                   WHERE PaymentOrderId = @ExistingPaymentOrderId
                     AND MessageType = 'RoutePayment'
               )
            BEGIN
                INSERT INTO RoutingOutboxMessages(PaymentOrderId,MessageType)
                VALUES(@ExistingPaymentOrderId,'RoutePayment');
            END;

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

        INSERT INTO RoutingOutboxMessages(PaymentOrderId,MessageType)
        VALUES(@PaymentOrderId,'RoutePayment');

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

-- ============================================================
-- SPs — ROUTING OUTBOX
-- ============================================================

CREATE OR ALTER PROCEDURE sp_RoutingOutbox_Pending
    @BatchSize int=10
AS
BEGIN
    SET NOCOUNT ON;

    IF @BatchSize<1 SET @BatchSize=1;
    IF @BatchSize>100 SET @BatchSize=100;

    SELECT TOP(@BatchSize)
           o.RoutingOutboxMessageId,
           o.AttemptCount,
           p.PaymentOrderId,
           p.CentralReference,
           p.SenderBankCode,
           p.ReceiverBankCode,
           p.ReceiverIban,
           p.ReceiverName,
           p.Amount,
           p.CurrencyCode,
           p.Description,
           b.ApiBaseUrl AS ReceiverApiBaseUrl
    FROM RoutingOutboxMessages o WITH(READPAST)
    INNER JOIN PaymentOrders p ON p.PaymentOrderId=o.PaymentOrderId
    INNER JOIN ParticipantBanks b ON b.BankCode=p.ReceiverBankCode
    WHERE o.MessageType='RoutePayment'
      AND o.ProcessedAtUtc IS NULL
      AND p.Status IN ('Accepted','Routing')
      AND b.IsActive=1
    ORDER BY o.CreatedAtUtc,o.RoutingOutboxMessageId;
END;
GO

CREATE OR ALTER PROCEDURE sp_RoutingOutbox_MarkRouting
    @RoutingOutboxMessageId int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @PaymentOrderId int;
    DECLARE @ProcessedAtUtc datetime2;
    DECLARE @Status nvarchar(30);

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @PaymentOrderId=PaymentOrderId,
               @ProcessedAtUtc=ProcessedAtUtc
        FROM RoutingOutboxMessages WITH(UPDLOCK,HOLDLOCK)
        WHERE RoutingOutboxMessageId=@RoutingOutboxMessageId
          AND MessageType='RoutePayment';

        IF @PaymentOrderId IS NULL
            THROW 50000,'Routing outbox message was not found.',1;

        IF @ProcessedAtUtc IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;
            RETURN;
        END;

        SELECT @Status=Status
        FROM PaymentOrders WITH(UPDLOCK,HOLDLOCK)
        WHERE PaymentOrderId=@PaymentOrderId;

        IF @Status='Accepted'
        BEGIN
            UPDATE PaymentOrders
            SET Status='Routing',
                RoutedAtUtc=SYSUTCDATETIME(),
                FailureReason=NULL
            WHERE PaymentOrderId=@PaymentOrderId;

            INSERT INTO PaymentStatusHistory(PaymentOrderId,Status)
            VALUES(@PaymentOrderId,'Routing');
        END
        ELSE IF @Status<>'Routing'
            THROW 50000,'Payment order is not available for routing.',1;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_RoutingOutbox_MarkResult
    @RoutingOutboxMessageId int,
    @ResultStatus nvarchar(30),
    @FailureReason nvarchar(500)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @PaymentOrderId int;
    DECLARE @ProcessedAtUtc datetime2;
    DECLARE @CurrentStatus nvarchar(30);

    IF @ResultStatus NOT IN ('Completed','Rejected')
        THROW 50000,'Recipient bank returned an unsupported payment status.',1;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @PaymentOrderId=PaymentOrderId,
               @ProcessedAtUtc=ProcessedAtUtc
        FROM RoutingOutboxMessages WITH(UPDLOCK,HOLDLOCK)
        WHERE RoutingOutboxMessageId=@RoutingOutboxMessageId
          AND MessageType='RoutePayment';

        IF @PaymentOrderId IS NULL
            THROW 50000,'Routing outbox message was not found.',1;

        IF @ProcessedAtUtc IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;
            RETURN;
        END;

        SELECT @CurrentStatus=Status
        FROM PaymentOrders WITH(UPDLOCK,HOLDLOCK)
        WHERE PaymentOrderId=@PaymentOrderId;

        IF @CurrentStatus NOT IN ('Accepted','Routing')
            THROW 50000,'Payment order is not waiting for a recipient result.',1;

        UPDATE PaymentOrders
        SET Status=@ResultStatus,
            CompletedAtUtc=CASE
                WHEN @ResultStatus='Completed' THEN SYSUTCDATETIME()
                ELSE NULL
            END,
            FailureReason=CASE
                WHEN @ResultStatus='Rejected' THEN @FailureReason
                ELSE NULL
            END
        WHERE PaymentOrderId=@PaymentOrderId;

        INSERT INTO PaymentStatusHistory(PaymentOrderId,Status,Reason)
        VALUES(
            @PaymentOrderId,
            @ResultStatus,
            CASE WHEN @ResultStatus='Rejected' THEN @FailureReason ELSE NULL END);

        UPDATE RoutingOutboxMessages
        SET ProcessedAtUtc=SYSUTCDATETIME(),
            AttemptCount=AttemptCount+1,
            LastError=NULL
        WHERE RoutingOutboxMessageId=@RoutingOutboxMessageId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_RoutingOutbox_MarkFailed
    @RoutingOutboxMessageId int,
    @Error nvarchar(1000),
    @MaxAttempts int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @PaymentOrderId int;
    DECLARE @AttemptCount int;

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @PaymentOrderId=PaymentOrderId,
               @AttemptCount=AttemptCount+1
        FROM RoutingOutboxMessages WITH(UPDLOCK,HOLDLOCK)
        WHERE RoutingOutboxMessageId=@RoutingOutboxMessageId
          AND MessageType='RoutePayment'
          AND ProcessedAtUtc IS NULL;

        IF @PaymentOrderId IS NULL
        BEGIN
            COMMIT TRANSACTION;
            RETURN;
        END;

        UPDATE RoutingOutboxMessages
        SET AttemptCount=@AttemptCount,
            LastError=@Error
        WHERE RoutingOutboxMessageId=@RoutingOutboxMessageId;

        IF @AttemptCount>=@MaxAttempts
        BEGIN
            UPDATE PaymentOrders
            SET Status='PendingReconciliation',
                FailureReason=@Error
            WHERE PaymentOrderId=@PaymentOrderId
              AND Status IN ('Accepted','Routing');

            IF @@ROWCOUNT=1
            BEGIN
                INSERT INTO PaymentStatusHistory(PaymentOrderId,Status,Reason)
                VALUES(@PaymentOrderId,'PendingReconciliation',@Error);
            END;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
