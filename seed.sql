-- BankApp Seed Data (IDENTITY auto-increment, no manual IDs)
-- Run AFTER migration.sql

-- 1. Branches
INSERT INTO Branches (BranchName, BranchCode, City, Address) VALUES
('Head Office',   'HO-001', 'Istanbul', 'Istiklal Cd. No:1 Beyoglu'),
('Ankara Branch', 'AN-002', 'Ankara',   'Kizilay Meydani No:12 Cankaya');

-- 2. Roles
INSERT INTO Roles (RoleName, Description) VALUES
('System Administrator', 'Full access to all modules'),
('Branch Manager',       'Manage branch operations'),
('Teller',               'Front-desk transactions');

-- 3. Currencies
INSERT INTO Currencies (CurrencyCode, CurrencyName) VALUES
('TRY', 'Turkish Lira'),
('USD', 'US Dollar'),
('EUR', 'Euro'),
('GBP', 'British Pound');

-- 4. Employees
INSERT INTO Employees (BranchId, RoleId, FirstName, LastName, Email, Phone, PasswordHash, AuthRole) VALUES
(1, 1, 'Emre', 'Admin', 'admin@bankapp.com', '555-0001',
 '$2a$11$RwoIN.Rcfbxw0lxRvcj/guxSbEfjDAt8f2DV9s4fKnQsf7dC42PrG', 'Admin'),
(1, 3, 'Jane', 'Teller', 'jane@bankapp.com', '555-0002',
 '$2a$11$GZPFsbHmZ1K5w3oELqGTOuQtMzwyJqOjg5O6ZaRUj5.7P4lC2nFPu', 'Employee');

-- 5. Customers
INSERT INTO Customers (FirstName, LastName, Email, Phone, Address, IsActive, PasswordHash) VALUES
('Ahmet',  'Yilmaz', 'ahmet@email.com',  '532-111-2233', 'Bagdat Cd. No:40 Kadikoy',          1, '$2a$11$DB/B1E/pQ4d0yoz.b7in8.ICAei/NPkxE8iczHZzRJ9EVO7mR4kNi'),
('Ayse',   'Demir',  'ayse@email.com',   '533-222-3344', 'Mesrutiyet Cd. No:18 Cankaya',      1, '$2a$11$DB/B1E/pQ4d0yoz.b7in8.ICAei/NPkxE8iczHZzRJ9EVO7mR4kNi'),
('Mehmet', 'Kaya',   'mehmet@email.com', NULL,           'Ataturk Bulvari No:75 Konak Izmir', 1, '$2a$11$DB/B1E/pQ4d0yoz.b7in8.ICAei/NPkxE8iczHZzRJ9EVO7mR4kNi'),
('Test', 'Cust',   'test@gmail.com', NULL,           'Ataturk Bulvari No:75 Konak Izm', 1, '$2a$11$DB/B1E/pQ4d0yoz.b7in8.ICAei/NPkxE8iczHZzRJ9EVO7mR4kNi');

-- 6. Accounts (reference customers by their known IDs from ORDER above: Ahmet=1, Ayse=2, Mehmet=3)
INSERT INTO Accounts (CustomerId, BranchId, CurrencyCode, Balance) VALUES
(1, 1, 'TRY', 15000.00),
(1, 1, 'USD',  2500.00),
(2, 2, 'TRY',  8500.50),
(4, 1, 'TRY', 95000);

-- 7. Transactions
INSERT INTO Transactions (AccountId, TransactionType, Amount, CurrencyCode, Description) VALUES
(1, 'Deposit',    5000.00, 'TRY', 'Initial deposit'),
(1, 'Withdrawal',  750.00, 'TRY', 'ATM withdrawal'),
(2, 'Deposit',    1000.00, 'USD', 'Wire transfer from abroad');

-- 8. Bills (all unpaid)
INSERT INTO Bills (CustomerId, BillType, Amount, CurrencyCode, DueDate, IsPaid) VALUES
(1, 'Electricity', 320.50, 'TRY', DATEADD(DAY, 15, GETDATE()), 0),
(1, 'Internet',    180.00, 'TRY', DATEADD(DAY, 10, GETDATE()), 0),
(2, 'Water',       145.75, 'TRY', DATEADD(DAY, 20, GETDATE()), 0),
(3, 'Gas',         890.00, 'TRY', DATEADD(DAY,  7, GETDATE()), 0);

-- 9. ExchangeRates
INSERT INTO ExchangeRates (CurrencyCode, Rate, Source) VALUES
('USD', 33.58, 'TCMB'),
('EUR', 36.92, 'TCMB'),
('GBP', 43.15, 'TCMB');

-- 10. System customer + holding accounts (for approval flow)
INSERT INTO Customers (FirstName, LastName, Email, Phone, Address, IsActive, PasswordHash)
VALUES ('System', 'Bank', 'system@bankapp.com', NULL, 'INTERNAL', 1, '-');

DECLARE @sysId int = SCOPE_IDENTITY();
INSERT INTO Accounts (CustomerId, BranchId, CurrencyCode, Balance) VALUES
(@sysId, 1, 'TRY', 0),
(@sysId, 1, 'USD', 0),
(@sysId, 1, 'EUR', 0),
(@sysId, 1, 'GBP', 0);

-- 11. Loan types
INSERT INTO LoanTypes (Name, AnnualInterestRate, MinAmount, MaxAmount, MinTermMonths, MaxTermMonths) VALUES
('Personal', 0.1500,  5000,   100000,  6,  36),
('Auto',     0.1200, 20000,   500000,  12, 60),
('Mortgage', 0.0900, 50000,  2000000, 36, 240);
