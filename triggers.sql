USE BankApp;
GO

-- Accounts: log old values on UPDATE/DELETE
CREATE TRIGGER trg_Accounts_History ON Accounts AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Accounts_History (OriginalId, OperationType, OperationTimeUtc, CustomerId, BranchId, CurrencyCode, Balance, CreatedDate)
    SELECT AccountId, 'D', GETDATE(), CustomerId, BranchId, CurrencyCode, Balance, CreatedDate FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.AccountId = DELETED.AccountId);
    INSERT INTO Accounts_History (OriginalId, OperationType, OperationTimeUtc, CustomerId, BranchId, CurrencyCode, Balance, CreatedDate)
    SELECT DELETED.AccountId, 'U', GETDATE(), DELETED.CustomerId, DELETED.BranchId, DELETED.CurrencyCode, DELETED.Balance, DELETED.CreatedDate
    FROM DELETED INNER JOIN INSERTED ON DELETED.AccountId = INSERTED.AccountId;
END;
GO

-- Bills
CREATE TRIGGER trg_Bills_History ON Bills AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Bills_History (OriginalId, OperationType, OperationTimeUtc, CustomerId, BillType, Amount, CurrencyCode, DueDate, IsPaid, PaidDate)
    SELECT BillId, 'D', GETDATE(), CustomerId, BillType, Amount, CurrencyCode, DueDate, IsPaid, PaidDate FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.BillId = DELETED.BillId);
    INSERT INTO Bills_History (OriginalId, OperationType, OperationTimeUtc, CustomerId, BillType, Amount, CurrencyCode, DueDate, IsPaid, PaidDate)
    SELECT d.BillId, 'U', GETDATE(), d.CustomerId, d.BillType, d.Amount, d.CurrencyCode, d.DueDate, d.IsPaid, d.PaidDate
    FROM DELETED d INNER JOIN INSERTED i ON d.BillId = i.BillId;
END;
GO

-- Branches
CREATE TRIGGER trg_Branches_History ON Branches AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Branches_History (OriginalId, OperationType, OperationTimeUtc, BranchName, BranchCode, City, Address, CreatedDate)
    SELECT BranchId, 'D', GETDATE(), BranchName, BranchCode, City, Address, CreatedDate FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.BranchId = DELETED.BranchId);
    INSERT INTO Branches_History (OriginalId, OperationType, OperationTimeUtc, BranchName, BranchCode, City, Address, CreatedDate)
    SELECT d.BranchId, 'U', GETDATE(), d.BranchName, d.BranchCode, d.City, d.Address, d.CreatedDate
    FROM DELETED d INNER JOIN INSERTED i ON d.BranchId = i.BranchId;
END;
GO

-- Currencies
CREATE TRIGGER trg_Currencies_History ON Currencies AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Currencies_History (OriginalId, OperationType, OperationTimeUtc, CurrencyName)
    SELECT CurrencyCode, 'D', GETDATE(), CurrencyName FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.CurrencyCode = DELETED.CurrencyCode);
    INSERT INTO Currencies_History (OriginalId, OperationType, OperationTimeUtc, CurrencyName)
    SELECT d.CurrencyCode, 'U', GETDATE(), d.CurrencyName
    FROM DELETED d INNER JOIN INSERTED i ON d.CurrencyCode = i.CurrencyCode;
END;
GO

-- Customers
CREATE TRIGGER trg_Customers_History ON Customers AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Customers_History (OriginalId, OperationType, OperationTimeUtc, FirstName, LastName, Email, Phone, Address, CreatedDate, IsActive, PasswordHash)
    SELECT CustomerId, 'D', GETDATE(), FirstName, LastName, Email, Phone, Address, CreatedDate, IsActive, PasswordHash FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.CustomerId = DELETED.CustomerId);
    INSERT INTO Customers_History (OriginalId, OperationType, OperationTimeUtc, FirstName, LastName, Email, Phone, Address, CreatedDate, IsActive, PasswordHash)
    SELECT d.CustomerId, 'U', GETDATE(), d.FirstName, d.LastName, d.Email, d.Phone, d.Address, d.CreatedDate, d.IsActive, d.PasswordHash
    FROM DELETED d INNER JOIN INSERTED i ON d.CustomerId = i.CustomerId;
END;
GO

-- Employees
CREATE TRIGGER trg_Employees_History ON Employees AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Employees_History (OriginalId, OperationType, OperationTimeUtc, BranchId, RoleId, FirstName, LastName, Email, Phone, HireDate, PasswordHash)
    SELECT EmployeeId, 'D', GETDATE(), BranchId, RoleId, FirstName, LastName, Email, Phone, HireDate, PasswordHash FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.EmployeeId = DELETED.EmployeeId);
    INSERT INTO Employees_History (OriginalId, OperationType, OperationTimeUtc, BranchId, RoleId, FirstName, LastName, Email, Phone, HireDate, PasswordHash)
    SELECT d.EmployeeId, 'U', GETDATE(), d.BranchId, d.RoleId, d.FirstName, d.LastName, d.Email, d.Phone, d.HireDate, d.PasswordHash
    FROM DELETED d INNER JOIN INSERTED i ON d.EmployeeId = i.EmployeeId;
END;
GO

-- ExchangeRates
CREATE TRIGGER trg_ExchangeRates_History ON ExchangeRates AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ExchangeRates_History (OriginalId, OperationType, OperationTimeUtc, CurrencyCode, Rate, RateDate, Source)
    SELECT RateId, 'D', GETDATE(), CurrencyCode, Rate, RateDate, Source FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.RateId = DELETED.RateId);
    INSERT INTO ExchangeRates_History (OriginalId, OperationType, OperationTimeUtc, CurrencyCode, Rate, RateDate, Source)
    SELECT d.RateId, 'U', GETDATE(), d.CurrencyCode, d.Rate, d.RateDate, d.Source
    FROM DELETED d INNER JOIN INSERTED i ON d.RateId = i.RateId;
END;
GO

-- Roles
CREATE TRIGGER trg_Roles_History ON Roles AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Roles_History (OriginalId, OperationType, OperationTimeUtc, RoleName, Description)
    SELECT RoleId, 'D', GETDATE(), RoleName, Description FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.RoleId = DELETED.RoleId);
    INSERT INTO Roles_History (OriginalId, OperationType, OperationTimeUtc, RoleName, Description)
    SELECT d.RoleId, 'U', GETDATE(), d.RoleName, d.Description
    FROM DELETED d INNER JOIN INSERTED i ON d.RoleId = i.RoleId;
END;
GO

-- Transactions
CREATE TRIGGER trg_Transactions_History ON Transactions AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Transactions_History (OriginalId, OperationType, OperationTimeUtc, AccountId, TransactionType, Amount, CurrencyCode, TransactionDate, Description)
    SELECT TransactionId, 'D', GETDATE(), AccountId, TransactionType, Amount, CurrencyCode, TransactionDate, Description FROM DELETED
    WHERE NOT EXISTS (SELECT 1 FROM INSERTED i WHERE i.TransactionId = DELETED.TransactionId);
    INSERT INTO Transactions_History (OriginalId, OperationType, OperationTimeUtc, AccountId, TransactionType, Amount, CurrencyCode, TransactionDate, Description)
    SELECT d.TransactionId, 'U', GETDATE(), d.AccountId, d.TransactionType, d.Amount, d.CurrencyCode, d.TransactionDate, d.Description
    FROM DELETED d INNER JOIN INSERTED i ON d.TransactionId = i.TransactionId;
END;
GO
