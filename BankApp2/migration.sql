USE BankApp2;
GO

DROP PROCEDURE IF EXISTS sp_IncomingPayments_Process;
GO

DROP TABLE IF EXISTS RecipientTransactions;
DROP TABLE IF EXISTS IncomingPayments;
DROP TABLE IF EXISTS RecipientAccounts;
GO

-- ============================================================
-- TABLES
-- ============================================================

CREATE TABLE RecipientAccounts (
    AccountId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Iban varchar(34) NOT NULL UNIQUE,
    AccountHolderName nvarchar(200) NOT NULL,
    Balance decimal(18,2) NOT NULL DEFAULT 0,
    CurrencyCode char(3) NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CONSTRAINT CK_RecipientAccounts_Balance_NonNegative CHECK (Balance>=0),
    CONSTRAINT CK_RecipientAccounts_Currency_TRY CHECK (CurrencyCode='TRY')
);
GO

CREATE TABLE IncomingPayments (
    IncomingPaymentId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CentralReference varchar(64) NOT NULL UNIQUE,
    RequestHash char(64) NOT NULL,
    SenderBankCode char(5) NOT NULL,
    ReceiverAccountId int NULL REFERENCES RecipientAccounts(AccountId),
    ReceiverIban varchar(34) NOT NULL,
    ReceiverName nvarchar(200) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode char(3) NOT NULL,
    Description nvarchar(255) NULL,
    Status nvarchar(30) NOT NULL,
    FailureReason nvarchar(500) NULL,
    ReceivedAtUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    ProcessedAtUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    CONSTRAINT CK_IncomingPayments_Amount_Positive CHECK (Amount>0),
    CONSTRAINT CK_IncomingPayments_Status CHECK (Status IN ('Completed','Rejected'))
);
GO

CREATE TABLE RecipientTransactions (
    TransactionId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    AccountId int NOT NULL REFERENCES RecipientAccounts(AccountId),
    CentralReference varchar(64) NOT NULL UNIQUE,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode char(3) NOT NULL,
    Description nvarchar(255) NULL,
    CreatedAtUtc datetime2 NOT NULL DEFAULT sysutcdatetime()
);
GO

-- ============================================================
-- TEST RECIPIENT
-- ============================================================

INSERT INTO RecipientAccounts(
    Iban,AccountHolderName,Balance,CurrencyCode)
VALUES(
    'TR120000200000000000000001','Test Receiver',200,'TRY');
GO

-- ============================================================
-- SPs — INCOMING PAYMENTS
-- ============================================================

CREATE OR ALTER PROCEDURE sp_IncomingPayments_Process
    @CentralReference varchar(64),
    @RequestHash char(64),
    @SenderBankCode char(5),
    @ReceiverIban varchar(34),
    @ReceiverName nvarchar(200),
    @Amount decimal(18,2),
    @CurrencyCode char(3),
    @Description nvarchar(255)=NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ExistingRequestHash char(64);
    DECLARE @AccountId int;
    DECLARE @FailureReason nvarchar(500);

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @ExistingRequestHash=RequestHash
        FROM IncomingPayments WITH(UPDLOCK,HOLDLOCK)
        WHERE CentralReference=@CentralReference;

        IF @ExistingRequestHash IS NOT NULL
        BEGIN
            IF @ExistingRequestHash<>@RequestHash
                THROW 50000,'Central reference was already used with different payment data.',1;

            SELECT CentralReference,Status,ProcessedAtUtc,FailureReason
            FROM IncomingPayments
            WHERE CentralReference=@CentralReference;

            COMMIT TRANSACTION;
            RETURN;
        END;

        SELECT @AccountId=AccountId
        FROM RecipientAccounts WITH(UPDLOCK,HOLDLOCK)
        WHERE Iban=@ReceiverIban
          AND CurrencyCode=@CurrencyCode
          AND IsActive=1;

        IF @AccountId IS NULL
        BEGIN
            SET @FailureReason='Receiver account was not found, is inactive, or uses another currency.';

            INSERT INTO IncomingPayments(
                CentralReference,RequestHash,SenderBankCode,ReceiverAccountId,
                ReceiverIban,ReceiverName,Amount,CurrencyCode,Description,
                Status,FailureReason)
            VALUES(
                @CentralReference,@RequestHash,@SenderBankCode,NULL,
                @ReceiverIban,@ReceiverName,@Amount,@CurrencyCode,@Description,
                'Rejected',@FailureReason);

            SELECT CentralReference,Status,ProcessedAtUtc,FailureReason
            FROM IncomingPayments
            WHERE CentralReference=@CentralReference;

            COMMIT TRANSACTION;
            RETURN;
        END;

        INSERT INTO IncomingPayments(
            CentralReference,RequestHash,SenderBankCode,ReceiverAccountId,
            ReceiverIban,ReceiverName,Amount,CurrencyCode,Description,Status)
        VALUES(
            @CentralReference,@RequestHash,@SenderBankCode,@AccountId,
            @ReceiverIban,@ReceiverName,@Amount,@CurrencyCode,@Description,'Completed');

        UPDATE RecipientAccounts
        SET Balance=Balance+@Amount
        WHERE AccountId=@AccountId;

        INSERT INTO RecipientTransactions(
            AccountId,CentralReference,Amount,CurrencyCode,Description)
        VALUES(
            @AccountId,@CentralReference,@Amount,@CurrencyCode,
            COALESCE(@Description,'Incoming EFT'));

        SELECT CentralReference,Status,ProcessedAtUtc,FailureReason
        FROM IncomingPayments
        WHERE CentralReference=@CentralReference;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
