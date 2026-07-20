USE BankApp;
GO

-- ============================================================
-- Drop all foreign keys
-- ============================================================
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT IF EXISTS ' + QUOTENAME(name) + '; '
FROM sys.foreign_keys;
EXEC sp_executesql @sql;
GO

-- ============================================================
-- Drop all tables
-- ============================================================
DROP TABLE IF EXISTS PendingTransfers;
DROP TABLE IF EXISTS Transactions_History;
DROP TABLE IF EXISTS Transactions;
DROP TABLE IF EXISTS Bills_History;
DROP TABLE IF EXISTS Bills;
DROP TABLE IF EXISTS ExchangeRates_History;
DROP TABLE IF EXISTS ExchangeRates;
DROP TABLE IF EXISTS Accounts_History;
DROP TABLE IF EXISTS Accounts;
DROP TABLE IF EXISTS Employees_History;
DROP TABLE IF EXISTS Employees;
DROP TABLE IF EXISTS Customers_History;
DROP TABLE IF EXISTS Customers;
DROP TABLE IF EXISTS Roles_History;
DROP TABLE IF EXISTS Roles;
DROP TABLE IF EXISTS Branches_History;
DROP TABLE IF EXISTS Branches;
DROP TABLE IF EXISTS Currencies_History;
DROP TABLE IF EXISTS Currencies;
DROP TABLE IF EXISTS LoanPayments;
DROP TABLE IF EXISTS LoanSchedules;
DROP TABLE IF EXISTS Loans;
DROP TABLE IF EXISTS LoanTypes;
GO

-- ============================================================
-- MAIN TABLES with IDENTITY
-- ============================================================

CREATE TABLE Currencies (
    CurrencyCode nvarchar(3) NOT NULL PRIMARY KEY,
    CurrencyName nvarchar(255) NOT NULL
);

CREATE TABLE Branches (
    BranchId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    BranchName nvarchar(255) NOT NULL,
    BranchCode nvarchar(255) NOT NULL,
    City nvarchar(255) NOT NULL,
    Address nvarchar(255) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT sysutcdatetime()
);

CREATE TABLE Roles (
    RoleId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleName nvarchar(255) NOT NULL,
    Description nvarchar(255) NULL
);

CREATE TABLE Customers (
    CustomerId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FirstName nvarchar(255) NOT NULL,
    LastName nvarchar(255) NOT NULL,
    Email nvarchar(255) NOT NULL UNIQUE,
    Phone nvarchar(255) NULL,
    Address nvarchar(255) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    IsActive bit NOT NULL,
    PasswordHash varchar(255) NOT NULL
);

CREATE TABLE Employees (
    EmployeeId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    BranchId int NOT NULL REFERENCES Branches(BranchId),
    RoleId int NOT NULL REFERENCES Roles(RoleId),
    FirstName nvarchar(255) NOT NULL,
    LastName nvarchar(255) NOT NULL,
    Email nvarchar(255) NOT NULL UNIQUE,
    Phone nvarchar(255) NOT NULL,
    HireDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    PasswordHash varchar(255) NOT NULL,
    AuthRole nvarchar(50) NOT NULL DEFAULT 'Employee'
);

CREATE TABLE Accounts (
    AccountId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerId int NOT NULL REFERENCES Customers(CustomerId),
    BranchId int NOT NULL REFERENCES Branches(BranchId),
    CurrencyCode nvarchar(3) NOT NULL REFERENCES Currencies(CurrencyCode),
    Balance decimal(18,2) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    IsActive bit NOT NULL DEFAULT 1
);

CREATE TABLE Transactions (
    TransactionId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    AccountId int NOT NULL REFERENCES Accounts(AccountId),
    TransactionType nvarchar(255) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode nvarchar(3) NOT NULL,
    TransactionDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    Description nvarchar(255) NULL,
    RelatedAccountId int NULL
);

CREATE TABLE ExchangeRates (
    RateId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CurrencyCode nvarchar(3) NOT NULL REFERENCES Currencies(CurrencyCode),
    Rate decimal(18,2) NOT NULL,
    RateDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    Source nvarchar(255) NOT NULL
);

CREATE TABLE Bills (
    BillId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerId int NOT NULL REFERENCES Customers(CustomerId),
    BillType nvarchar(255) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode nvarchar(3) NULL,
    DueDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    IsPaid bit NOT NULL,
    PaidDate datetime2 NULL DEFAULT sysutcdatetime()
);

CREATE TABLE PendingTransfers (
    PendingTransferId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SourceAccountId int NOT NULL,
    TargetAccountId int NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode nvarchar(3) NOT NULL,
    Description nvarchar(255) NULL,
    Status nvarchar(20) NOT NULL DEFAULT 'Pending',
    CreatedByCustomerId int NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT sysutcdatetime(),
    ResolvedByEmployeeId int NULL,
    ResolvedAt datetime2 NULL,
    RejectionReason nvarchar(255) NULL
);
GO

-- ============================================================
-- LOAN TABLES
-- ============================================================

CREATE TABLE LoanTypes (
    LoanTypeId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name nvarchar(100) NOT NULL,
    AnnualInterestRate decimal(6,4) NOT NULL,
    MinAmount decimal(18,2) NOT NULL,
    MaxAmount decimal(18,2) NOT NULL,
    MinTermMonths int NOT NULL,
    MaxTermMonths int NOT NULL,
    IsActive bit NOT NULL DEFAULT 1
);

CREATE TABLE Loans (
    LoanId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CustomerId int NOT NULL REFERENCES Customers(CustomerId),
    LoanTypeId int NOT NULL REFERENCES LoanTypes(LoanTypeId),
    Amount decimal(18,2) NOT NULL,
    TermMonths int NOT NULL,
    AnnualInterestRate decimal(6,4) NOT NULL,
    MonthlyPayment decimal(18,2) NOT NULL,
    DisbursementAccountId int NOT NULL REFERENCES Accounts(AccountId),
    PaymentAccountId int NOT NULL REFERENCES Accounts(AccountId),
    Status nvarchar(20) NOT NULL DEFAULT 'Pending',
    AppliedAt datetime2 NOT NULL DEFAULT sysutcdatetime(),
    ApprovedAt datetime2 NULL,
    ClosedAt datetime2 NULL,
    PaymentsMade int NOT NULL DEFAULT 0,
    PaymentsMissed int NOT NULL DEFAULT 0,
    RemainingPrincipal decimal(18,2) NOT NULL DEFAULT 0
);

CREATE TABLE LoanSchedules (
    ScheduleId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LoanId int NOT NULL REFERENCES Loans(LoanId),
    PeriodNumber int NOT NULL,
    DueDate datetime2 NOT NULL,
    Principal decimal(18,2) NOT NULL,
    Interest decimal(18,2) NOT NULL,
    TotalDue decimal(18,2) NOT NULL,
    RemainingBalance decimal(18,2) NOT NULL,
    IsPaid bit NOT NULL DEFAULT 0,
    PaidDate datetime2 NULL,
    IsLate bit NOT NULL DEFAULT 0
);

CREATE TABLE LoanPayments (
    PaymentId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ScheduleId int NULL REFERENCES LoanSchedules(ScheduleId),
    LoanId int NOT NULL REFERENCES Loans(LoanId),
    Amount decimal(18,2) NOT NULL,
    PaymentType nvarchar(20) NOT NULL,
    PaymentDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    Description nvarchar(255) NULL
);
GO

-- ============================================================
-- HISTORY TABLES (from original create.sql)
-- ============================================================

CREATE TABLE Currencies_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId nvarchar(3) NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    CurrencyName nvarchar(255) NOT NULL
);
CREATE INDEX IX_Currencies_History_OriginalId ON Currencies_History(OriginalId);
CREATE INDEX IX_Currencies_History_OperationTimeUtc ON Currencies_History(OperationTimeUtc);

CREATE TABLE Branches_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    BranchName nvarchar(255) NOT NULL,
    BranchCode nvarchar(255) NOT NULL,
    City nvarchar(255) NOT NULL,
    Address nvarchar(255) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT sysutcdatetime()
);
CREATE INDEX IX_Branches_History_OriginalId ON Branches_History(OriginalId);
CREATE INDEX IX_Branches_History_OperationTimeUtc ON Branches_History(OperationTimeUtc);

CREATE TABLE Roles_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    RoleName nvarchar(255) NOT NULL,
    Description nvarchar(255) NULL
);
CREATE INDEX IX_Roles_History_OriginalId ON Roles_History(OriginalId);
CREATE INDEX IX_Roles_History_OperationTimeUtc ON Roles_History(OperationTimeUtc);

CREATE TABLE Customers_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    FirstName nvarchar(255) NOT NULL,
    LastName nvarchar(255) NOT NULL,
    Email nvarchar(255) NOT NULL,
    Phone nvarchar(255) NULL,
    Address nvarchar(255) NOT NULL,
    CreatedDate datetime2 NOT NULL DEFAULT sysutcdatetime(),
    IsActive bit NOT NULL,
    PasswordHash varchar(255) NOT NULL
);
CREATE INDEX IX_Customers_History_OriginalId ON Customers_History(OriginalId);
CREATE INDEX IX_Customers_History_OperationTimeUtc ON Customers_History(OperationTimeUtc);

CREATE TABLE Employees_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    BranchId int NOT NULL,
    RoleId int NOT NULL,
    FirstName nvarchar(255) NOT NULL,
    LastName nvarchar(255) NOT NULL,
    Email nvarchar(255) NOT NULL,
    Phone nvarchar(255) NOT NULL,
    HireDate datetime2 NOT NULL,
    PasswordHash varchar(255) NOT NULL
);
CREATE INDEX IX_Employees_History_OriginalId ON Employees_History(OriginalId);
CREATE INDEX IX_Employees_History_OperationTimeUtc ON Employees_History(OperationTimeUtc);

CREATE TABLE Accounts_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    CustomerId int NOT NULL,
    BranchId int NOT NULL,
    CurrencyCode nvarchar(3) NOT NULL,
    Balance decimal(18,2) NOT NULL,
    CreatedDate datetime2 NOT NULL,
    IsActive bit NOT NULL DEFAULT 1
);
CREATE INDEX IX_Accounts_History_OriginalId ON Accounts_History(OriginalId);
CREATE INDEX IX_Accounts_History_OperationTimeUtc ON Accounts_History(OperationTimeUtc);

CREATE TABLE Transactions_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    AccountId int NOT NULL,
    TransactionType nvarchar(255) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode nvarchar(3) NOT NULL,
    TransactionDate datetime2 NOT NULL,
    Description nvarchar(255) NULL,
    RelatedAccountId int NULL
);
CREATE INDEX IX_Transactions_History_OriginalId ON Transactions_History(OriginalId);
CREATE INDEX IX_Transactions_History_OperationTimeUtc ON Transactions_History(OperationTimeUtc);

CREATE TABLE ExchangeRates_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    CurrencyCode nvarchar(3) NOT NULL,
    Rate decimal(18,2) NOT NULL,
    RateDate datetime2 NOT NULL,
    Source nvarchar(255) NOT NULL
);
CREATE INDEX IX_ExchangeRates_History_OriginalId ON ExchangeRates_History(OriginalId);
CREATE INDEX IX_ExchangeRates_History_OperationTimeUtc ON ExchangeRates_History(OperationTimeUtc);

CREATE TABLE Bills_History (
    HistoryId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OriginalId int NOT NULL,
    OperationType nvarchar(1) NOT NULL,
    OperationTimeUtc datetime2 NOT NULL DEFAULT sysutcdatetime(),
    OperationUser nvarchar(255) NULL,
    CustomerId int NOT NULL,
    BillType nvarchar(255) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    CurrencyCode nvarchar(3) NULL,
    DueDate datetime2 NOT NULL,
    IsPaid bit NOT NULL,
    PaidDate datetime2 NULL
);
CREATE INDEX IX_Bills_History_OriginalId ON Bills_History(OriginalId);
CREATE INDEX IX_Bills_History_OperationTimeUtc ON Bills_History(OperationTimeUtc);
GO

-- ============================================================
-- TRIGGERS — write old values to history on UPDATE/DELETE
-- ============================================================
CREATE TRIGGER trg_Accounts_History ON Accounts AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Accounts_History(OriginalId,OperationType,OperationTimeUtc,CustomerId,BranchId,CurrencyCode,Balance,CreatedDate,IsActive)SELECT AccountId,'D',GETDATE(),CustomerId,BranchId,CurrencyCode,Balance,CreatedDate,IsActive FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.AccountId=DELETED.AccountId);
    INSERT INTO Accounts_History(OriginalId,OperationType,OperationTimeUtc,CustomerId,BranchId,CurrencyCode,Balance,CreatedDate,IsActive)SELECT d.AccountId,'U',GETDATE(),d.CustomerId,d.BranchId,d.CurrencyCode,d.Balance,d.CreatedDate,d.IsActive FROM DELETED d INNER JOIN INSERTED i ON d.AccountId=i.AccountId;
END;
GO
CREATE TRIGGER trg_Bills_History ON Bills AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Bills_History(OriginalId,OperationType,OperationTimeUtc,CustomerId,BillType,Amount,CurrencyCode,DueDate,IsPaid,PaidDate)SELECT BillId,'D',GETDATE(),CustomerId,BillType,Amount,CurrencyCode,DueDate,IsPaid,PaidDate FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.BillId=DELETED.BillId);
    INSERT INTO Bills_History(OriginalId,OperationType,OperationTimeUtc,CustomerId,BillType,Amount,CurrencyCode,DueDate,IsPaid,PaidDate)SELECT d.BillId,'U',GETDATE(),d.CustomerId,d.BillType,d.Amount,d.CurrencyCode,d.DueDate,d.IsPaid,d.PaidDate FROM DELETED d INNER JOIN INSERTED i ON d.BillId=i.BillId;
END;
GO
CREATE TRIGGER trg_Branches_History ON Branches AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Branches_History(OriginalId,OperationType,OperationTimeUtc,BranchName,BranchCode,City,Address,CreatedDate)SELECT BranchId,'D',GETDATE(),BranchName,BranchCode,City,Address,CreatedDate FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.BranchId=DELETED.BranchId);
    INSERT INTO Branches_History(OriginalId,OperationType,OperationTimeUtc,BranchName,BranchCode,City,Address,CreatedDate)SELECT d.BranchId,'U',GETDATE(),d.BranchName,d.BranchCode,d.City,d.Address,d.CreatedDate FROM DELETED d INNER JOIN INSERTED i ON d.BranchId=i.BranchId;
END;
GO
CREATE TRIGGER trg_Currencies_History ON Currencies AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Currencies_History(OriginalId,OperationType,OperationTimeUtc,CurrencyName)SELECT CurrencyCode,'D',GETDATE(),CurrencyName FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.CurrencyCode=DELETED.CurrencyCode);
    INSERT INTO Currencies_History(OriginalId,OperationType,OperationTimeUtc,CurrencyName)SELECT d.CurrencyCode,'U',GETDATE(),d.CurrencyName FROM DELETED d INNER JOIN INSERTED i ON d.CurrencyCode=i.CurrencyCode;
END;
GO
CREATE TRIGGER trg_Customers_History ON Customers AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Customers_History(OriginalId,OperationType,OperationTimeUtc,FirstName,LastName,Email,Phone,Address,CreatedDate,IsActive,PasswordHash)SELECT CustomerId,'D',GETDATE(),FirstName,LastName,Email,Phone,Address,CreatedDate,IsActive,PasswordHash FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.CustomerId=DELETED.CustomerId);
    INSERT INTO Customers_History(OriginalId,OperationType,OperationTimeUtc,FirstName,LastName,Email,Phone,Address,CreatedDate,IsActive,PasswordHash)SELECT d.CustomerId,'U',GETDATE(),d.FirstName,d.LastName,d.Email,d.Phone,d.Address,d.CreatedDate,d.IsActive,d.PasswordHash FROM DELETED d INNER JOIN INSERTED i ON d.CustomerId=i.CustomerId;
END;
GO
CREATE TRIGGER trg_Employees_History ON Employees AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Employees_History(OriginalId,OperationType,OperationTimeUtc,BranchId,RoleId,FirstName,LastName,Email,Phone,HireDate,PasswordHash)SELECT EmployeeId,'D',GETDATE(),BranchId,RoleId,FirstName,LastName,Email,Phone,HireDate,PasswordHash FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.EmployeeId=DELETED.EmployeeId);
    INSERT INTO Employees_History(OriginalId,OperationType,OperationTimeUtc,BranchId,RoleId,FirstName,LastName,Email,Phone,HireDate,PasswordHash)SELECT d.EmployeeId,'U',GETDATE(),d.BranchId,d.RoleId,d.FirstName,d.LastName,d.Email,d.Phone,d.HireDate,d.PasswordHash FROM DELETED d INNER JOIN INSERTED i ON d.EmployeeId=i.EmployeeId;
END;
GO
CREATE TRIGGER trg_ExchangeRates_History ON ExchangeRates AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ExchangeRates_History(OriginalId,OperationType,OperationTimeUtc,CurrencyCode,Rate,RateDate,Source)SELECT RateId,'D',GETDATE(),CurrencyCode,Rate,RateDate,Source FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.RateId=DELETED.RateId);
    INSERT INTO ExchangeRates_History(OriginalId,OperationType,OperationTimeUtc,CurrencyCode,Rate,RateDate,Source)SELECT d.RateId,'U',GETDATE(),d.CurrencyCode,d.Rate,d.RateDate,d.Source FROM DELETED d INNER JOIN INSERTED i ON d.RateId=i.RateId;
END;
GO
CREATE TRIGGER trg_Roles_History ON Roles AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Roles_History(OriginalId,OperationType,OperationTimeUtc,RoleName,Description)SELECT RoleId,'D',GETDATE(),RoleName,Description FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.RoleId=DELETED.RoleId);
    INSERT INTO Roles_History(OriginalId,OperationType,OperationTimeUtc,RoleName,Description)SELECT d.RoleId,'U',GETDATE(),d.RoleName,d.Description FROM DELETED d INNER JOIN INSERTED i ON d.RoleId=i.RoleId;
END;
GO
CREATE TRIGGER trg_Transactions_History ON Transactions AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Transactions_History(OriginalId,OperationType,OperationTimeUtc,AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)SELECT TransactionId,'D',GETDATE(),AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId FROM DELETED WHERE NOT EXISTS(SELECT 1 FROM INSERTED i WHERE i.TransactionId=DELETED.TransactionId);
    INSERT INTO Transactions_History(OriginalId,OperationType,OperationTimeUtc,AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)SELECT d.TransactionId,'U',GETDATE(),d.AccountId,d.TransactionType,d.Amount,d.CurrencyCode,d.TransactionDate,d.Description,d.RelatedAccountId FROM DELETED d INNER JOIN INSERTED i ON d.TransactionId=i.TransactionId;
END;
GO

-- ============================================================
-- Drop all existing stored procedures
-- ============================================================
DECLARE @dropSql NVARCHAR(MAX) = '';
SELECT @dropSql = @dropSql + 'DROP PROCEDURE IF EXISTS ' + QUOTENAME(s.name) + '.' + QUOTENAME(p.name) + '; '
FROM sys.procedures p
JOIN sys.schemas s ON p.schema_id = s.schema_id;
EXEC sp_executesql @dropSql;
GO

-- ============================================================
-- SPs — Accounts
-- ============================================================
CREATE PROCEDURE sp_Accounts_List
AS
BEGIN
    SELECT * FROM Accounts ORDER BY AccountId;
END;
GO
CREATE PROCEDURE sp_Accounts_Select @AccountId int
AS
BEGIN
    SELECT * FROM Accounts WHERE AccountId=@AccountId;
END;
GO
CREATE PROCEDURE sp_Accounts_Insert @CustomerId int,@BranchId int,@CurrencyCode nvarchar(3),@Balance decimal(18,2)
AS
BEGIN
    INSERT INTO Accounts(CustomerId,BranchId,CurrencyCode,Balance)VALUES(@CustomerId,@BranchId,@CurrencyCode,@Balance);
    SELECT SCOPE_IDENTITY()AS AccountId;
END;
GO
CREATE PROCEDURE sp_Accounts_Update @AccountId int,@CustomerId int,@BranchId int,@CurrencyCode nvarchar(3),@Balance decimal(18,2)
AS
BEGIN
    UPDATE Accounts SET CustomerId=@CustomerId,BranchId=@BranchId,CurrencyCode=@CurrencyCode,Balance=@Balance WHERE AccountId=@AccountId;
END;
GO
CREATE PROCEDURE sp_Accounts_Delete @AccountId int
AS
BEGIN
    UPDATE Accounts SET IsActive=0 WHERE AccountId=@AccountId;
END;
GO

-- ============================================================
-- SPs — Bills
-- ============================================================
CREATE PROCEDURE sp_Bills_List
AS
BEGIN
    SELECT * FROM Bills ORDER BY DueDate DESC;
END;
GO
CREATE PROCEDURE sp_Bills_Select @BillId int
AS
BEGIN
    SELECT * FROM Bills WHERE BillId=@BillId;
END;
GO
CREATE PROCEDURE sp_Bills_Insert @CustomerId int,@BillType nvarchar(255),@Amount decimal(18,2),@CurrencyCode nvarchar(3)=NULL,@DueDate datetime2,@IsPaid bit,@PaidDate datetime2=NULL
AS
BEGIN
    INSERT INTO Bills(CustomerId,BillType,Amount,CurrencyCode,DueDate,IsPaid,PaidDate)VALUES(@CustomerId,@BillType,@Amount,@CurrencyCode,@DueDate,@IsPaid,@PaidDate);
    SELECT SCOPE_IDENTITY()AS BillId;
END;
GO
CREATE PROCEDURE sp_Bills_Update @BillId int,@CustomerId int,@BillType nvarchar(255),@Amount decimal(18,2),@CurrencyCode nvarchar(3)=NULL,@DueDate datetime2,@IsPaid bit,@PaidDate datetime2=NULL
AS
BEGIN
    UPDATE Bills SET CustomerId=@CustomerId,BillType=@BillType,Amount=@Amount,CurrencyCode=@CurrencyCode,DueDate=@DueDate,IsPaid=@IsPaid,PaidDate=@PaidDate WHERE BillId=@BillId;
END;
GO
CREATE PROCEDURE sp_Bills_Delete @BillId int
AS
BEGIN
    DELETE FROM Bills WHERE BillId=@BillId;
END;
GO
CREATE PROCEDURE sp_Bills_MarkPaid @BillId int
AS
BEGIN
    UPDATE Bills SET IsPaid=1,PaidDate=GETDATE()WHERE BillId=@BillId;
END;
GO

-- ============================================================
-- SPs — Branches
-- ============================================================
CREATE PROCEDURE sp_Branches_List
AS
BEGIN
    SELECT * FROM Branches ORDER BY BranchId;
END;
GO
CREATE PROCEDURE sp_Branches_Select @BranchId int
AS
BEGIN
    SELECT * FROM Branches WHERE BranchId=@BranchId;
END;
GO
CREATE PROCEDURE sp_Branches_Insert @BranchName nvarchar(255),@BranchCode nvarchar(255),@City nvarchar(255),@Address nvarchar(255)
AS
BEGIN
    INSERT INTO Branches(BranchName,BranchCode,City,Address)VALUES(@BranchName,@BranchCode,@City,@Address);
    SELECT SCOPE_IDENTITY()AS BranchId;
END;
GO
CREATE PROCEDURE sp_Branches_Update @BranchId int,@BranchName nvarchar(255),@BranchCode nvarchar(255),@City nvarchar(255),@Address nvarchar(255)
AS
BEGIN
    UPDATE Branches SET BranchName=@BranchName,BranchCode=@BranchCode,City=@City,Address=@Address WHERE BranchId=@BranchId;
END;
GO
CREATE PROCEDURE sp_Branches_Delete @BranchId int
AS
BEGIN
    DELETE FROM Branches WHERE BranchId=@BranchId;
END;
GO

-- ============================================================
-- SPs — Currencies
-- ============================================================
CREATE PROCEDURE sp_Currencies_List
AS
BEGIN
    SELECT * FROM Currencies ORDER BY CurrencyCode;
END;
GO
CREATE PROCEDURE sp_Currencies_Select @CurrencyCode nvarchar(3)
AS
BEGIN
    SELECT * FROM Currencies WHERE CurrencyCode=@CurrencyCode;
END;
GO
CREATE PROCEDURE sp_Currencies_Insert @CurrencyCode nvarchar(3),@CurrencyName nvarchar(255)
AS
BEGIN
    INSERT INTO Currencies(CurrencyCode,CurrencyName)VALUES(@CurrencyCode,@CurrencyName);
END;
GO
CREATE PROCEDURE sp_Currencies_Update @CurrencyCode nvarchar(3),@CurrencyName nvarchar(255)
AS
BEGIN
    UPDATE Currencies SET CurrencyName=@CurrencyName WHERE CurrencyCode=@CurrencyCode;
END;
GO
CREATE PROCEDURE sp_Currencies_Delete @CurrencyCode nvarchar(3)
AS
BEGIN
    DELETE FROM Currencies WHERE CurrencyCode=@CurrencyCode;
END;
GO

-- ============================================================
-- SPs — Customers
-- ============================================================
CREATE PROCEDURE sp_Customers_List
AS
BEGIN
    SELECT * FROM Customers ORDER BY CustomerId;
END;
GO
CREATE PROCEDURE sp_Customers_Select @CustomerId int
AS
BEGIN
    SELECT * FROM Customers WHERE CustomerId=@CustomerId;
END;
GO
CREATE PROCEDURE sp_Customers_Insert @FirstName nvarchar(255),@LastName nvarchar(255),@Email nvarchar(255),@Phone nvarchar(255)=NULL,@Address nvarchar(255),@IsActive bit,@PasswordHash varchar(255)
AS
BEGIN
    INSERT INTO Customers(FirstName,LastName,Email,Phone,Address,IsActive,PasswordHash)VALUES(@FirstName,@LastName,@Email,@Phone,@Address,@IsActive,@PasswordHash);
    SELECT SCOPE_IDENTITY()AS CustomerId;
END;
GO
CREATE PROCEDURE sp_Customers_Update @CustomerId int,@FirstName nvarchar(255),@LastName nvarchar(255),@Email nvarchar(255),@Phone nvarchar(255)=NULL,@Address nvarchar(255),@IsActive bit,@PasswordHash varchar(255)
AS
BEGIN
    UPDATE Customers SET FirstName=@FirstName,LastName=@LastName,Email=@Email,Phone=@Phone,Address=@Address,IsActive=@IsActive,PasswordHash=@PasswordHash WHERE CustomerId=@CustomerId;
END;
GO
CREATE PROCEDURE sp_Customers_Delete @CustomerId int
AS
BEGIN
    UPDATE Accounts SET IsActive=0 WHERE CustomerId=@CustomerId;
    UPDATE Customers SET IsActive=0 WHERE CustomerId=@CustomerId;
END;
GO

-- ============================================================
-- SPs — Employees
-- ============================================================
CREATE PROCEDURE sp_Employees_List
AS
BEGIN
    SELECT * FROM Employees ORDER BY EmployeeId;
END;
GO
CREATE PROCEDURE sp_Employees_Select @EmployeeId int
AS
BEGIN
    SELECT * FROM Employees WHERE EmployeeId=@EmployeeId;
END;
GO
CREATE PROCEDURE sp_Employees_Insert @BranchId int,@RoleId int,@FirstName nvarchar(255),@LastName nvarchar(255),@Email nvarchar(255),@Phone nvarchar(255),@PasswordHash varchar(255),@AuthRole nvarchar(50)
AS
BEGIN
    INSERT INTO Employees(BranchId,RoleId,FirstName,LastName,Email,Phone,PasswordHash,AuthRole)VALUES(@BranchId,@RoleId,@FirstName,@LastName,@Email,@Phone,@PasswordHash,@AuthRole);
    SELECT SCOPE_IDENTITY()AS EmployeeId;
END;
GO
CREATE PROCEDURE sp_Employees_Update @EmployeeId int,@BranchId int,@RoleId int,@FirstName nvarchar(255),@LastName nvarchar(255),@Email nvarchar(255),@Phone nvarchar(255),@PasswordHash varchar(255),@AuthRole nvarchar(50)
AS
BEGIN
    UPDATE Employees SET BranchId=@BranchId,RoleId=@RoleId,FirstName=@FirstName,LastName=@LastName,Email=@Email,Phone=@Phone,PasswordHash=@PasswordHash,AuthRole=@AuthRole WHERE EmployeeId=@EmployeeId;
END;
GO
CREATE PROCEDURE sp_Employees_Delete @EmployeeId int
AS
BEGIN
    DELETE FROM Employees WHERE EmployeeId=@EmployeeId;
END;
GO

-- ============================================================
-- SPs — ExchangeRates
-- ============================================================
CREATE PROCEDURE sp_ExchangeRates_List
AS
BEGIN
    SELECT * FROM ExchangeRates ORDER BY RateDate DESC;
END;
GO
CREATE PROCEDURE sp_ExchangeRates_Select @RateId int
AS
BEGIN
    SELECT * FROM ExchangeRates WHERE RateId=@RateId;
END;
GO
CREATE PROCEDURE sp_ExchangeRates_Insert @CurrencyCode nvarchar(3),@Rate decimal(18,2),@Source nvarchar(255)
AS
BEGIN
    INSERT INTO ExchangeRates(CurrencyCode,Rate,Source)VALUES(@CurrencyCode,@Rate,@Source);
    SELECT SCOPE_IDENTITY()AS RateId;
END;
GO
CREATE PROCEDURE sp_ExchangeRates_Update @RateId int,@CurrencyCode nvarchar(3),@Rate decimal(18,2),@Source nvarchar(255)
AS
BEGIN
    UPDATE ExchangeRates SET CurrencyCode=@CurrencyCode,Rate=@Rate,Source=@Source WHERE RateId=@RateId;
END;
GO
CREATE PROCEDURE sp_ExchangeRates_Delete @RateId int
AS
BEGIN
    DELETE FROM ExchangeRates WHERE RateId=@RateId;
END;
GO

-- ============================================================
-- SPs — Roles
-- ============================================================
CREATE PROCEDURE sp_Roles_List
AS
BEGIN
    SELECT * FROM Roles ORDER BY RoleId;
END;
GO
CREATE PROCEDURE sp_Roles_Select @RoleId int
AS
BEGIN
    SELECT * FROM Roles WHERE RoleId=@RoleId;
END;
GO
CREATE PROCEDURE sp_Roles_Insert @RoleName nvarchar(255),@Description nvarchar(255)=NULL
AS
BEGIN
    INSERT INTO Roles(RoleName,Description)VALUES(@RoleName,@Description);
    SELECT SCOPE_IDENTITY()AS RoleId;
END;
GO
CREATE PROCEDURE sp_Roles_Update @RoleId int,@RoleName nvarchar(255),@Description nvarchar(255)=NULL
AS
BEGIN
    UPDATE Roles SET RoleName=@RoleName,Description=@Description WHERE RoleId=@RoleId;
END;
GO
CREATE PROCEDURE sp_Roles_Delete @RoleId int
AS
BEGIN
    DELETE FROM Roles WHERE RoleId=@RoleId;
END;
GO

-- ============================================================
-- SPs — Transactions
-- ============================================================
CREATE PROCEDURE sp_Transactions_List
AS
BEGIN
    SELECT * FROM Transactions ORDER BY TransactionDate DESC;
END;
GO
CREATE PROCEDURE sp_Transactions_Select @TransactionId int
AS
BEGIN
    SELECT * FROM Transactions WHERE TransactionId=@TransactionId;
END;
GO
CREATE PROCEDURE sp_Transactions_Insert @AccountId int,@TransactionType nvarchar(255),@Amount decimal(18,2),@CurrencyCode nvarchar(3),@Description nvarchar(255)=NULL
AS
BEGIN
    INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,Description)VALUES(@AccountId,@TransactionType,@Amount,@CurrencyCode,@Description);
    SELECT SCOPE_IDENTITY()AS TransactionId;
END;
GO
CREATE PROCEDURE sp_Transactions_Update @TransactionId int,@AccountId int,@TransactionType nvarchar(255),@Amount decimal(18,2),@CurrencyCode nvarchar(3),@Description nvarchar(255)=NULL
AS
BEGIN
    UPDATE Transactions SET AccountId=@AccountId,TransactionType=@TransactionType,Amount=@Amount,CurrencyCode=@CurrencyCode,Description=@Description WHERE TransactionId=@TransactionId;
END;
GO
CREATE PROCEDURE sp_Transactions_Delete @TransactionId int
AS
BEGIN
    DELETE FROM Transactions WHERE TransactionId=@TransactionId;
END;
GO

-- ============================================================
-- SPs — Customer Portal
-- ============================================================
CREATE PROCEDURE sp_Customer_Accounts @CustomerId int
AS
BEGIN
    SELECT * FROM Accounts WHERE CustomerId=@CustomerId ORDER BY IsActive DESC,AccountId;
END;
GO
CREATE PROCEDURE sp_Customer_Transactions @CustomerId int
AS
BEGIN
    SELECT t.* FROM Transactions t INNER JOIN Accounts a ON t.AccountId=a.AccountId WHERE a.CustomerId=@CustomerId ORDER BY t.TransactionDate DESC;
END;
GO
CREATE PROCEDURE sp_Customer_Bills @CustomerId int
AS
BEGIN
    SELECT * FROM Bills WHERE CustomerId=@CustomerId ORDER BY DueDate DESC;
END;
GO
CREATE PROCEDURE sp_Customer_CreateAccount @CustomerId int,@BranchId int,@CurrencyCode nvarchar(3)
AS
BEGIN
    INSERT INTO Accounts(CustomerId,BranchId,CurrencyCode,Balance,CreatedDate)VALUES(@CustomerId,@BranchId,@CurrencyCode,0,GETDATE());
    SELECT SCOPE_IDENTITY()AS AccountId;
END;
GO

CREATE PROCEDURE sp_Customer_TransferWithHold
    @CustomerId int,@SourceAccountId int,@TargetAccountId int,
    @Amount decimal(18,2),@Description nvarchar(255)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SrcCid int,@SrcBal decimal(18,2),@SrcCur nvarchar(3),@TgtCur nvarchar(3);
    DECLARE @HoldingAccountId int,@SysCid int;
    DECLARE @Threshold decimal(18,2)=5000;
    DECLARE @PendingTransferId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @SrcCid=CustomerId,@SrcBal=Balance,@SrcCur=CurrencyCode FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@SourceAccountId AND IsActive=1;
        IF @SrcCid IS NULL OR @SrcCid!=@CustomerId BEGIN RAISERROR('Source account does not belong to you.',16,1);RETURN;END;
        IF @SrcBal<@Amount BEGIN RAISERROR('Insufficient balance.',16,1);RETURN;END;
        SELECT @TgtCur=CurrencyCode FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@TargetAccountId;
        IF @TgtCur IS NULL BEGIN RAISERROR('Target account not found.',16,1);RETURN;END;
        IF @SrcCur!=@TgtCur BEGIN RAISERROR('Cannot transfer between different currencies.',16,1);RETURN;END;
        IF @Amount<=@Threshold BEGIN
            UPDATE Accounts SET Balance=Balance-@Amount WHERE AccountId=@SourceAccountId;
            UPDATE Accounts SET Balance=Balance+@Amount WHERE AccountId=@TargetAccountId;
            INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@SourceAccountId,'Withdrawal',@Amount,@SrcCur,GETDATE(),@Description,@TargetAccountId);
            INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@TargetAccountId,'Deposit',@Amount,@TgtCur,GETDATE(),@Description,@SourceAccountId);
            SELECT 'Completed' AS TransferStatus,CAST(NULL AS int)AS PendingTransferId;
        END ELSE BEGIN
            SELECT @SysCid=CustomerId FROM Customers WHERE Email='system@bankapp.com';
            SELECT TOP 1 @HoldingAccountId=AccountId FROM Accounts WHERE CustomerId=@SysCid AND CurrencyCode=@SrcCur;
            IF @HoldingAccountId IS NULL BEGIN RAISERROR('System holding account not configured.',16,1);RETURN;END;
            UPDATE Accounts SET Balance=Balance-@Amount WHERE AccountId=@SourceAccountId;
            UPDATE Accounts SET Balance=Balance+@Amount WHERE AccountId=@HoldingAccountId;
            INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@SourceAccountId,'Withdrawal',@Amount,@SrcCur,GETDATE(),CONCAT(@Description,' - Pending approval'),@TargetAccountId);
            INSERT INTO PendingTransfers(SourceAccountId,TargetAccountId,Amount,CurrencyCode,Description,Status,CreatedByCustomerId)VALUES(@SourceAccountId,@TargetAccountId,@Amount,@SrcCur,@Description,'Pending',@CustomerId);
            SET @PendingTransferId=SCOPE_IDENTITY();
            SELECT 'Pending' AS TransferStatus,@PendingTransferId AS PendingTransferId;
        END;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

CREATE PROCEDURE sp_Customer_PayBill @CustomerId int,@BillId int,@AccountId int=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @BCid int,@Paid bit,@BAmt decimal(18,2),@BCur nvarchar(3),@AccId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @BCid=CustomerId,@Paid=IsPaid,@BAmt=Amount,@BCur=CurrencyCode FROM Bills WITH(UPDLOCK,HOLDLOCK)WHERE BillId=@BillId;
        IF @BCid IS NULL OR @BCid!=@CustomerId BEGIN RAISERROR('Bill does not belong to you.',16,1);RETURN;END;
        IF @Paid=1 BEGIN RAISERROR('Bill is already paid.',16,1);RETURN;END;
        IF @AccountId IS NOT NULL BEGIN
            SELECT @AccId=AccountId FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@AccountId AND CustomerId=@CustomerId AND CurrencyCode=@BCur AND Balance>=@BAmt AND IsActive=1;
            IF @AccId IS NULL BEGIN RAISERROR('Selected account is not eligible for this bill payment.',16,1);RETURN;END;
        END ELSE BEGIN
            SELECT TOP 1 @AccId=AccountId FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE CustomerId=@CustomerId AND CurrencyCode=@BCur AND Balance>=@BAmt AND IsActive=1 ORDER BY Balance DESC;
            IF @AccId IS NULL BEGIN RAISERROR('No account with sufficient balance for this currency.',16,1);RETURN;END;
        END;
        UPDATE Accounts SET Balance=Balance-@BAmt WHERE AccountId=@AccId;
        UPDATE Bills SET IsPaid=1,PaidDate=GETDATE()WHERE BillId=@BillId;
        INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description)VALUES(@AccId,'Withdrawal',@BAmt,@BCur,GETDATE(),'Bill payment');
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

CREATE PROCEDURE sp_Customer_Exchange
    @CustomerId int,@SourceAccountId int,@TargetAccountId int,@TargetAmount decimal(18,2)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SrcCid int,@SrcBal decimal(18,2),@SrcCur nvarchar(3);
    DECLARE @TgtCid int,@TgtCur nvarchar(3);
    DECLARE @SrcRate decimal(18,2),@TgtRate decimal(18,2),@SourceAmount decimal(18,2);
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @SrcCid=CustomerId,@SrcBal=Balance,@SrcCur=CurrencyCode FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@SourceAccountId AND IsActive=1;
        IF @SrcCid IS NULL OR @SrcCid!=@CustomerId BEGIN RAISERROR('Source account does not belong to you.',16,1);RETURN;END;
        SELECT @TgtCid=CustomerId,@TgtCur=CurrencyCode FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@TargetAccountId AND IsActive=1;
        IF @TgtCid IS NULL OR @TgtCid!=@CustomerId BEGIN RAISERROR('Target account does not belong to you.',16,1);RETURN;END;
        IF @SrcCur=@TgtCur BEGIN RAISERROR('Source and target must have different currencies.',16,1);RETURN;END;
        IF @SrcCur='TRY' SET @SrcRate=1.0; ELSE SELECT @SrcRate=Rate FROM ExchangeRates WHERE CurrencyCode=@SrcCur ORDER BY RateDate DESC;
        IF @TgtCur='TRY' SET @TgtRate=1.0; ELSE SELECT @TgtRate=Rate FROM ExchangeRates WHERE CurrencyCode=@TgtCur ORDER BY RateDate DESC;
        IF @SrcRate IS NULL OR @SrcRate<=0 OR @TgtRate IS NULL OR @TgtRate<=0 BEGIN RAISERROR('No exchange rate found.',16,1);RETURN;END;
        SET @SourceAmount=@TargetAmount*(@TgtRate/@SrcRate);
        IF @SrcBal<@SourceAmount BEGIN RAISERROR('Insufficient balance.',16,1);RETURN;END;
        UPDATE Accounts SET Balance=Balance-@SourceAmount WHERE AccountId=@SourceAccountId;
        UPDATE Accounts SET Balance=Balance+@TargetAmount WHERE AccountId=@TargetAccountId;
        INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@SourceAccountId,'Withdrawal',@SourceAmount,@SrcCur,GETDATE(),CONCAT('Forex: sold ',@SourceAmount,' ',@SrcCur,' for ',@TargetAmount,' ',@TgtCur),@TargetAccountId);
        INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@TargetAccountId,'Deposit',@TargetAmount,@TgtCur,GETDATE(),CONCAT('Forex: bought with ',@SrcCur),@SourceAccountId);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

-- ============================================================
-- SPs — Approval Flow
-- ============================================================
CREATE PROCEDURE sp_ApproveTransfer @PendingTransferId int,@EmployeeId int
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SrcId int,@TgtId int,@Amt decimal(18,2),@Cur nvarchar(3),@Desc nvarchar(255);
    DECLARE @HoldingId int,@SysCid int;
    DECLARE @CreatedByCustomerId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @SrcId=SourceAccountId,@TgtId=TargetAccountId,@Amt=Amount,@Cur=CurrencyCode,@Desc=Description,@CreatedByCustomerId=CreatedByCustomerId FROM PendingTransfers WITH(UPDLOCK,HOLDLOCK)WHERE PendingTransferId=@PendingTransferId AND Status='Pending';
        IF @SrcId IS NULL BEGIN RAISERROR('Pending transfer not found or already processed.',16,1);RETURN;END;
        IF NOT EXISTS(SELECT 1 FROM Accounts WHERE AccountId=@TgtId AND IsActive=1)BEGIN RAISERROR('Target account is deactivated.',16,1);RETURN;END;
        SELECT @SysCid=CustomerId FROM Customers WHERE Email='system@bankapp.com';
        SELECT TOP 1 @HoldingId=AccountId FROM Accounts WHERE CustomerId=@SysCid AND CurrencyCode=@Cur;
        UPDATE Accounts SET Balance=Balance-@Amt WHERE AccountId=@HoldingId;
        UPDATE Accounts SET Balance=Balance+@Amt WHERE AccountId=@TgtId;
        INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@TgtId,'Deposit',@Amt,@Cur,GETDATE(),CONCAT(@Desc,' - Approved'),@SrcId);
        UPDATE PendingTransfers SET Status='Approved',ResolvedByEmployeeId=@EmployeeId,ResolvedAt=GETDATE()WHERE PendingTransferId=@PendingTransferId;
        COMMIT TRANSACTION;
        SELECT @CreatedByCustomerId AS CreatedByCustomerId;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

CREATE PROCEDURE sp_RejectTransfer @PendingTransferId int,@EmployeeId int,@Reason nvarchar(255)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SrcId int,@Amt decimal(18,2),@Cur nvarchar(3),@Desc nvarchar(255);
    DECLARE @HoldingId int,@SysCid int;
    DECLARE @CreatedByCustomerId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @SrcId=SourceAccountId,@Amt=Amount,@Cur=CurrencyCode,@Desc=Description,@CreatedByCustomerId=CreatedByCustomerId FROM PendingTransfers WITH(UPDLOCK,HOLDLOCK)WHERE PendingTransferId=@PendingTransferId AND Status='Pending';
        IF @SrcId IS NULL BEGIN RAISERROR('Pending transfer not found or already processed.',16,1);RETURN;END;
        SELECT @SysCid=CustomerId FROM Customers WHERE Email='system@bankapp.com';
        SELECT TOP 1 @HoldingId=AccountId FROM Accounts WHERE CustomerId=@SysCid AND CurrencyCode=@Cur;
        UPDATE Accounts SET Balance=Balance-@Amt WHERE AccountId=@HoldingId;
        UPDATE Accounts SET Balance=Balance+@Amt WHERE AccountId=@SrcId;
        INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description)VALUES(@SrcId,'Deposit',@Amt,@Cur,GETDATE(),CONCAT(@Desc,' - Rejected: ',ISNULL(@Reason,'')));
        UPDATE PendingTransfers SET Status='Rejected',ResolvedByEmployeeId=@EmployeeId,ResolvedAt=GETDATE(),RejectionReason=@Reason WHERE PendingTransferId=@PendingTransferId;
        COMMIT TRANSACTION;
        SELECT @CreatedByCustomerId AS CreatedByCustomerId;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

CREATE PROCEDURE sp_Admin_PendingTransfers
AS
BEGIN
    SELECT pt.*,
           src.FirstName AS SrcFirstName,src.LastName AS SrcLastName,
           tgt.FirstName AS TgtFirstName,tgt.LastName AS TgtLastName
    FROM PendingTransfers pt
    LEFT JOIN Accounts sa ON pt.SourceAccountId=sa.AccountId
    LEFT JOIN Customers src ON sa.CustomerId=src.CustomerId
    LEFT JOIN Accounts ta ON pt.TargetAccountId=ta.AccountId
    LEFT JOIN Customers tgt ON ta.CustomerId=tgt.CustomerId
    WHERE pt.Status='Pending'
    ORDER BY pt.CreatedAt DESC;
END;
GO

CREATE PROCEDURE sp_Account_Lookup @AccountId int
AS
BEGIN
    SELECT c.FirstName, c.LastName
    FROM Accounts a INNER JOIN Customers c ON a.CustomerId=c.CustomerId
    WHERE a.AccountId=@AccountId;
END;
GO

CREATE PROCEDURE sp_Account_RecentTransfers @AccountId int
AS
BEGIN
    SELECT TOP 3 t.TransactionId,t.AccountId,t.TransactionType,t.Amount,t.CurrencyCode,t.TransactionDate,t.Description,t.RelatedAccountId,c.FirstName,c.LastName,ra.CurrencyCode AS RelatedCurrencyCode
    FROM Transactions t LEFT JOIN Accounts ra ON t.RelatedAccountId=ra.AccountId LEFT JOIN Customers c ON ra.CustomerId=c.CustomerId
    WHERE t.AccountId=@AccountId AND t.RelatedAccountId IS NOT NULL
    ORDER BY t.TransactionDate DESC;
END;
GO

CREATE PROCEDURE sp_Account_TransferBetween
    @SourceAccountId int,@TargetAccountId int,
    @Amount decimal(18,2),@Description nvarchar(255)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SrcCur nvarchar(3),@SrcBal decimal(18,2),@TgtCur nvarchar(3);
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @SrcCur=CurrencyCode,@SrcBal=Balance FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@SourceAccountId;
        IF @SrcCur IS NULL BEGIN RAISERROR('Source account not found.',16,1);RETURN;END;
        IF @SrcBal<@Amount BEGIN RAISERROR('Insufficient balance.',16,1);RETURN;END;
        SELECT @TgtCur=CurrencyCode FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@TargetAccountId;
        IF @TgtCur IS NULL BEGIN RAISERROR('Target account not found.',16,1);RETURN;END;
        IF @SrcCur!=@TgtCur BEGIN RAISERROR('Currency codes do not match.',16,1);RETURN;END;
        UPDATE Accounts SET Balance=Balance-@Amount WHERE AccountId=@SourceAccountId;
        UPDATE Accounts SET Balance=Balance+@Amount WHERE AccountId=@TargetAccountId;
        INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@SourceAccountId,'Withdrawal',@Amount,@SrcCur,GETDATE(),@Description,@TargetAccountId);
        INSERT INTO Transactions(AccountId,TransactionType,Amount,CurrencyCode,TransactionDate,Description,RelatedAccountId)VALUES(@TargetAccountId,'Deposit',@Amount,@TgtCur,GETDATE(),@Description,@SourceAccountId);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

-- ============================================================
-- SPs — Loans
-- ============================================================

CREATE PROCEDURE sp_LoanTypes_List
AS
BEGIN
    SELECT * FROM LoanTypes WHERE IsActive=1 ORDER BY LoanTypeId;
END;
GO

CREATE PROCEDURE sp_Loans_List
AS
BEGIN
    SELECT l.*,c.FirstName AS CustomerFirstName,c.LastName AS CustomerLastName,
           lt.Name AS LoanTypeName
    FROM Loans l
    INNER JOIN Customers c ON l.CustomerId=c.CustomerId
    INNER JOIN LoanTypes lt ON l.LoanTypeId=lt.LoanTypeId
    ORDER BY l.LoanId DESC;
END;
GO

CREATE PROCEDURE sp_Loans_Select @LoanId int
AS
BEGIN
    SELECT l.*,c.FirstName AS CustomerFirstName,c.LastName AS CustomerLastName,
           lt.Name AS LoanTypeName,
           srcAcc.Balance AS DisbursementBalance,payAcc.Balance AS PaymentBalance
    FROM Loans l
    INNER JOIN Customers c ON l.CustomerId=c.CustomerId
    INNER JOIN LoanTypes lt ON l.LoanTypeId=lt.LoanTypeId
    LEFT JOIN Accounts srcAcc ON l.DisbursementAccountId=srcAcc.AccountId
    LEFT JOIN Accounts payAcc ON l.PaymentAccountId=payAcc.AccountId
    WHERE l.LoanId=@LoanId;
END;
GO

CREATE PROCEDURE sp_Customer_Loans @CustomerId int
AS
BEGIN
    SELECT l.*,lt.Name AS LoanTypeName
    FROM Loans l
    INNER JOIN LoanTypes lt ON l.LoanTypeId=lt.LoanTypeId
    WHERE l.CustomerId=@CustomerId
    ORDER BY l.LoanId DESC;
END;
GO

CREATE PROCEDURE sp_Loans_Apply
    @CustomerId int,@LoanTypeId int,@Amount decimal(18,2),
    @TermMonths int,@DisbursementAccountId int,@PaymentAccountId int
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @SrcBal decimal(18,2),@Dbal decimal(18,2),@Pbal decimal(18,2);
    DECLARE @MinAmt decimal(18,2),@MaxAmt decimal(18,2),@MinTerm int,@MaxTerm int;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @MinAmt=MinAmount,@MaxAmt=MaxAmount,@MinTerm=MinTermMonths,@MaxTerm=MaxTermMonths
        FROM LoanTypes WITH(UPDLOCK,HOLDLOCK)WHERE LoanTypeId=@LoanTypeId AND IsActive=1;
        IF @MinAmt IS NULL BEGIN RAISERROR('Loan type not found.',16,1);RETURN;END;
        IF @Amount<@MinAmt OR @Amount>@MaxAmt BEGIN RAISERROR('Amount outside allowed range.',16,1);RETURN;END;
        IF @TermMonths<@MinTerm OR @TermMonths>@MaxTerm BEGIN RAISERROR('Term outside allowed range.',16,1);RETURN;END;
        SELECT @Dbal=Balance FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@DisbursementAccountId AND CustomerId=@CustomerId AND IsActive=1;
        IF @Dbal IS NULL BEGIN RAISERROR('Disbursement account invalid.',16,1);RETURN;END;
        SELECT @Pbal=Balance FROM Accounts WITH(UPDLOCK,HOLDLOCK)WHERE AccountId=@PaymentAccountId AND CustomerId=@CustomerId AND IsActive=1;
        IF @Pbal IS NULL BEGIN RAISERROR('Payment account invalid.',16,1);RETURN;END;
        SELECT @SrcBal=SUM(Balance)FROM Accounts WHERE CustomerId=@CustomerId AND IsActive=1;
        IF @SrcBal<@Amount*0.3 BEGIN RAISERROR('Insufficient total balance for loan eligibility.',16,1);RETURN;END;
        INSERT INTO Loans(CustomerId,LoanTypeId,Amount,TermMonths,AnnualInterestRate,MonthlyPayment,DisbursementAccountId,PaymentAccountId,Status)
        SELECT @CustomerId,@LoanTypeId,@Amount,@TermMonths,AnnualInterestRate,0,@DisbursementAccountId,@PaymentAccountId,'Pending'
        FROM LoanTypes WHERE LoanTypeId=@LoanTypeId;
        SELECT SCOPE_IDENTITY()AS LoanId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

CREATE PROCEDURE sp_Loans_Reject @LoanId int,@EmployeeId int,@Reason nvarchar(255)=NULL
AS
BEGIN
    UPDATE Loans SET Status='Rejected' WHERE LoanId=@LoanId AND Status='Pending';
END;
GO

CREATE PROCEDURE sp_Loans_GetSchedule @LoanId int
AS
BEGIN
    SELECT * FROM LoanSchedules WHERE LoanId=@LoanId ORDER BY PeriodNumber;
END;
GO

CREATE PROCEDURE sp_Loans_MakePayment @LoanId int,@ScheduleId int,@AccountId int
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Total decimal(18,2),@Paid bit,@LoanStatus nvarchar(20),@RemainingSchedules int;
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @Total=TotalDue,@Paid=IsPaid FROM LoanSchedules WITH(UPDLOCK,HOLDLOCK)WHERE ScheduleId=@ScheduleId AND LoanId=@LoanId;
        IF @Total IS NULL BEGIN RAISERROR('Schedule not found.',16,1);RETURN;END;
        IF @Paid=1 BEGIN RAISERROR('Schedule already paid.',16,1);RETURN;END;
        UPDATE Accounts SET Balance=Balance-@Total WHERE AccountId=@AccountId AND Balance>=@Total;
        IF @@ROWCOUNT=0 BEGIN RAISERROR('Insufficient balance.',16,1);RETURN;END;
        UPDATE LoanSchedules SET IsPaid=1,PaidDate=GETDATE()WHERE ScheduleId=@ScheduleId;
        UPDATE Loans SET PaymentsMade=PaymentsMade+1,RemainingPrincipal=RemainingPrincipal-(SELECT Principal FROM LoanSchedules WHERE ScheduleId=@ScheduleId)WHERE LoanId=@LoanId;
        INSERT INTO LoanPayments(ScheduleId,LoanId,Amount,PaymentType,Description)
        VALUES(@ScheduleId,@LoanId,@Total,'Scheduled','Period payment');
        SELECT @RemainingSchedules=COUNT(*)FROM LoanSchedules WHERE LoanId=@LoanId AND IsPaid=0;
        IF @RemainingSchedules=0 UPDATE Loans SET Status='Paid',ClosedAt=GETDATE(),RemainingPrincipal=0 WHERE LoanId=@LoanId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

CREATE PROCEDURE sp_Loans_CloseEarly @LoanId int,@AccountId int
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @RemainingPrin decimal(18,2),@Penalty decimal(18,2),@Total decimal(18,2);
    BEGIN TRY
        BEGIN TRANSACTION;
        SELECT @RemainingPrin=SUM(Principal)FROM LoanSchedules WITH(UPDLOCK,HOLDLOCK)WHERE LoanId=@LoanId AND IsPaid=0;
        IF @RemainingPrin IS NULL OR @RemainingPrin<=0 BEGIN RAISERROR('No remaining balance.',16,1);RETURN;END;
        SET @Penalty=@RemainingPrin*0.02;
        SET @Total=@RemainingPrin+@Penalty;
        UPDATE Accounts SET Balance=Balance-@Total WHERE AccountId=@AccountId AND Balance>=@Total;
        IF @@ROWCOUNT=0 BEGIN RAISERROR('Insufficient balance for early closure.',16,1);RETURN;END;
        UPDATE LoanSchedules SET IsPaid=1,PaidDate=GETDATE()WHERE LoanId=@LoanId AND IsPaid=0;
        UPDATE Loans SET Status='Paid',ClosedAt=GETDATE(),RemainingPrincipal=0,PaymentsMade=(SELECT COUNT(*)FROM LoanSchedules WHERE LoanId=@LoanId)WHERE LoanId=@LoanId;
        INSERT INTO LoanPayments(LoanId,Amount,PaymentType,Description)
        VALUES(@LoanId,@RemainingPrin,'Early','Early closure: principal');
        INSERT INTO LoanPayments(LoanId,Amount,PaymentType,Description)
        VALUES(@LoanId,@Penalty,'Penalty','Early closure penalty: 2%');
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;THROW; END CATCH
END;
GO

CREATE PROCEDURE sp_Loans_GetPayments @LoanId int
AS
BEGIN
    SELECT * FROM LoanPayments WHERE LoanId=@LoanId ORDER BY PaymentDate DESC;
END;
GO

CREATE PROCEDURE sp_Loans_DueSchedules
AS
BEGIN
    SELECT s.*,l.CustomerId,l.PaymentAccountId,
           a.CurrencyCode AS PaymentCurrency,a.Balance AS PaymentBalance,
           c.FirstName,c.LastName
    FROM LoanSchedules s
    INNER JOIN Loans l ON s.LoanId=l.LoanId AND l.Status='Active'
    LEFT JOIN Accounts a ON l.PaymentAccountId=a.AccountId
    INNER JOIN Customers c ON l.CustomerId=c.CustomerId
    WHERE s.DueDate<=GETUTCDATE() AND s.IsPaid=0 AND s.IsLate=0
    ORDER BY s.DueDate;
END;
GO

-- ============================================================
-- End of migration
-- ============================================================
