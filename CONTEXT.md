# BankApp — Full-Stack Banking Application

## Stack
- **Backend:** ASP.NET Core 10.0, raw SQL via Microsoft.Data.SqlClient (no ORM), JWT auth
- **Frontend:** React 19 + TypeScript 6, Vite 8, react-router-dom 7
- **Database:** SQL Server (localhost, Windows Auth), all IDs use IDENTITY(1,1)

## Project structure
```
Desktop/BankApp/
  Controllers/          — 12 API controllers (1 per entity + Auth + CustomerPortal + AdminApproval)
  BankApp.Services/     — Business logic layer
  BankApp.DataAccess/   — Raw SQL data access (SqlConnection + Stored Procedures)
  BankApp.Entity/       — Domain models
  BankApp.Common/       — DTOs, interfaces, Result<T> pattern, enums, helpers
  Program.cs            — DI, JWT setup, Swagger, CORS
  migration.sql         — Complete DB setup (tables, SPs, triggers, FKs)
  seed.sql              — Mock data (branches, roles, customers, accounts, bills, rates, system account)
  Client/               — React frontend
    src/
      types/index.ts    — All TypeScript types
      services/         — API clients (api.ts base + per-entity + customer.ts + admin.ts)
      context/AuthContext.tsx — Auth state, login/logout
      components/       — Layout, Sidebar, Topbar, CustomerLayout, CustomerSidebar, icons, ui
      pages/
        LoginPage, ForgotPasswordPage, ResetPasswordPage, DashboardPage
        modules/        — Admin CRUD pages (Customers, Accounts, Bills, Branches, Roles, Currencies, Employees, ExchangeRates, Transactions, Approvals)
        customer/       — Customer portal (Dashboard, MyAccounts, Transfer, Exchange, PayBills, ExchangeRates)
      App.tsx           — Router with role-based Protected() guard
```

## Auth flow
```
Login → AuthService checks Employees table first, then Customers
      → PasswordHasher.Verify()
      → JwtTokenService.CreateToken() creates 5 claims: sub, email, nameidentifier, name, role
      → Frontend stores token + user info in localStorage
      → Every subsequent request: api.ts adds "Authorization: Bearer <token>"
      → All controllers except AuthController have [Authorize(Roles = "Admin,Employee")]
      → CustomerPortalController has [Authorize(Roles = "Customer")]
      → User ID extracted in controller via: User.FindFirst(ClaimTypes.NameIdentifier).Value
```

## Authorization pattern
- Admin controllers: `[Authorize(Roles = "Admin,Employee")]` at class level
- Customer controller: `[Authorize(Roles = "Customer")]` at class level
- No manual claim reading in services — controller extracts userId and passes it down

## Database conventions
- All PKs are IDENTITY(1,1), auto-generated
- All SPs use `SCOPE_IDENTITY()` on INSERT
- All tables have matching `_History` tables with triggers logging UPDATE/DELETE old values
- No FK cascades — all deletes handled in SPs/C# logic
- Currency codes are 3-char nvarchar (TRY, USD, EUR, GBP)

## SP naming conventions
- `sp_{Entity}_{Action}` for CRUD: List, Select, Insert, Update, Delete
- `sp_Customer_{Action}` for customer portal queries
- `sp_Customer_TransferWithHold` — transfer with >5000 threshold approval
- `sp_Customer_PayBill`, `sp_Customer_Exchange` — financial SPs
- `sp_ApproveTransfer`, `sp_RejectTransfer` — admin approval SPs
- `sp_Admin_PendingTransfers` — list pending
- `sp_Account_Lookup`, `sp_Account_RecentTransfers` — lookup helpers
- `sp_Bills_MarkPaid` — dedicated mark-paid SP

## Financial SP safety patterns
All financial SPs (TransferWithHold, PayBill, Exchange, ApproveTransfer, RejectTransfer) use:
```sql
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    SELECT ... FROM table WITH (UPDLOCK, HOLDLOCK) WHERE ...;  -- row locks
    -- validation checks with RAISERROR
    -- atomic UPDATEs + INSERTs
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
```

## Key features implemented
1. **Role-based routing** — App.tsx Protected() checks user.role → Customer gets CustomerLayout, admin/employee gets Layout
2. **Approval flow** — Transfers >5000 go to system holding account, PendingTransfers table, admin Approve/Reject
3. **Currency exchange** — Any pair via TRY intermediary, rate from ExchangeRates table
4. **Bill payment** — Auto-detects matching-currency account with sufficient balance
5. **Account owner lookup** — 800ms debounce, masked name display (**met **maz)
6. **Recent transfers display** — Shows last 3 transfers with counterpart on Transfer page
7. **History tables** — 9 triggers auto-log UPDATE/DELETE to _{table}_History tables
8. **RelatedAccountId** — Links transfer/exchange pairs in Transactions table

## Frontend patterns
- Every service call returns `Result<T>` → checked with `if (!result.Success) return StatusCode(..., result); return Ok(result)`
- AuthContext wraps the app, useAuth() hook available everywhere
- Module pages use `useSearchParams()` for sub-action routing (?action=List|Create|Edit|Detail)
- Customer pages use plain routes (/customer/accounts, /customer/transfer, etc.)
- Date formatting: `formatDate()` from @/components/ui → `14.07.2026 23:50`
- Name masking: `maskName()` from @/components/ui → hides first 2 chars
- `@/` alias resolves to `src/` (configured in vite.config.ts + tsconfig)

## Current running state
- Backend: http://localhost:5000
- Frontend: http://localhost:5173 (npm run dev in Client/)
- Database: BankApp on localhost, Windows Auth
- Admin login: admin@bankapp.com (hash: $2a$11$RwoIN.Rcfbxw0lxRvcj/guxSbEfjDAt8f2DV9s4fKnQsf7dC42PrG)
- Customer logins: ahmet@email.com, ayse@email.com, mehmet@email.com (hash: $2a$11$DB/B1E/pQ4d0yoz.b7in8.ICAei/NPkxE8iczHZzRJ9EVO7mR4kNi)
- System customer: system@bankapp.com (for approval holding accounts)

## Key files to know
- `migration.sql` — drop + recreate everything (tables, SPs, triggers, FKs). Has DROP PROCEDURE IF EXISTS before SP creation.
- `seed.sql` — populate mock data. Run AFTER migration.sql.
- `Program.cs` — DI registrations, JWT config
- `Client/src/App.tsx` — routes + Protected guard
- `Client/src/services/api.ts` — fetch wrapper with JWT header injection
- `sp_Approval.sql` — TransferWithHold + Approve + Reject SPs
