# README improvement

- [x] Use a general internship-project description without naming the company
- [x] Add the WPF admin screenshot alongside the web application screenshots
- [x] Improve stack, architecture, feature, prerequisite, and setup documentation
- [x] Explain local secret configuration without exposing credentials
- [x] Verify all referenced files and screenshot paths exist

## Review

- Rewrote the README as a concise portfolio-style project page.
- Grouped four web screenshots in tables and added the WPF administration screenshot.
- Added architecture, project structure, prerequisites, and separate launch instructions.
- Confirmed all screenshot paths exist and `git diff --check` reports no errors.

# Istanbul long-term software internship search

- [x] Search official career sites across banking, fintech, telecom, e-commerce, automotive/industrial, airlines, defense, consulting, and large technology employers
- [x] Verify that each candidate has a live company-hosted or company-linked application path on 2026-07-28
- [x] Check Istanbul/remote eligibility, software relevance, long-term or part-time evidence, deadline, and status
- [x] Separate confirmed openings from general talent forms and stale/closed program pages
- [x] Deduplicate and rank the confirmed opportunities

## Review

- Searched Turkish and English title variants, including working student, candidate engineer, young talent, co-op, IT intern, trainee, and mandatory/voluntary long-term internship.
- Ranked current openings by technical fit for a final-year YTU Electronics and Communication Engineering student with C#/.NET, React, TypeScript, and SQL experience.
- Verified application state against detail pages and employer career systems where available.
- Flagged search-index contradictions, repost duplicates, expired deadlines, and listings whose stated start date has already passed.

# Istanbul European-side long-term internship search

- [x] Search live software, backend, IT, working-student, and long-term internship listings by European-side district
- [x] Search employer career pages and startup listings outside the major job-board results
- [x] Verify visible application state, work model, weekly availability, and location
- [x] Rank confirmed roles by technical fit and practical travel from Beylikdüzü
- [x] Separate confirmed openings from undated forms, uncertain mirrors, and closed listings

## Review

- Verified the application button and detail-page status for the strongest current matches on 2026-07-28.
- Prioritized remote roles and Bağcılar, Beylikdüzü, Zeytinburnu, Şişli, Kabataş, and Sarıyer locations.
- Separated direct software roles from IT-support roles and marked listings with unclear duration, pay, or total weekly workload.
- Excluded or downgraded listings whose exact detail pages were closed, expired, undated, or located on the Anatolian side.

# Beylikdüzü-area software opportunity search

- [x] Search Beylikdüzü, Esenyurt, Büyükçekmece, Haramidere, and Avcılar for live software vacancies
- [x] Find active software companies and startups without advertised vacancies
- [x] Verify current office location, products, technical relevance, and a public contact route
- [x] Exclude generic IT support and companies supported only by stale directory entries
- [x] Separate live applications from cold outreach and rank by Metrobus accessibility and fit

## Review

- Inspected the current Entertech Istanbul Technopark and YTU Yildiz Technopark company directories on 2026-07-28.
- Cross-checked shortlisted firms against their own live product, contact, career, and public company pages.
- Ranked small fintech, backend, data, and AI product teams by technical fit and location confidence.
- Kept directory residency separate from confirmed daily office presence and marked conflicting or incomplete address data.

# Küçükçekmece-Basın Ekspres software opportunity search

- [x] Search current long-term internships and part-time roles in the requested districts
- [x] Research active software firms around Beşyol, Florya, Yenibosna, and Zaim Teknopark
- [x] Verify each office address, current product work, technical fit, and public contact route
- [x] Separate live application routes from cold-outreach targets and closed listings
- [x] Rank the final shortlist by C#/.NET/SQL relevance and practical Metrobus access

## Review

- Verified three currently actionable vacancy pages and one official internship intake route on 2026-07-28.
- Found a useful local product-company cluster at Zaim Teknopark plus strong targets in Kartaltepe, Beşyol, Florya, and Yenibosna.
- Kept Message34's closed .NET internship only as evidence for a targeted cold approach, not as a live vacancy.
- Excluded companies with stale, conflicting, or non-local addresses and listings whose office location could not be verified.

# 2026-07-29 Beylikduzu-first internship search

- [x] Search and verify open software internships in Beylikduzu, Esenyurt, Avcilar, and Buyukcekmece
- [x] Search verified openings farther along the Metrobus corridor and wider Istanbul
- [x] Find active nearby startup and medium-company cold-email targets
- [x] Verify exact application status, office, technical relevance, and public contact route
- [x] Rank results by commute, software fit, and realistic value for a junior candidate

## Review

- Verified direct application state and deadlines on 2026-07-29, separating live openings, rolling application routes, cold-outreach targets, and closed listings.
- Prioritized Beylikduzu, Zeytinburnu, Bagcilar, Bahcesehir, Yenibosna, and remote work before roles on the Anatolian side.
- Downgraded unpaid, IT-support, eligibility-mismatched, and location-uncertain opportunities.
- Built a first-wave cold-outreach list around nearby companies with current or recent C#/.NET, SQL, React, payments, ERP, or integration work.

# 2026-07-29 strict C#/.NET opportunity search

- [x] Search current Istanbul and Turkey-remote listings that explicitly require C# or .NET
- [x] Exclude generic frontend, full-stack, backend, IT, and language-agnostic roles
- [x] Exclude every company and role already presented or applied to
- [x] Verify direct-page application status, location, seniority, and student eligibility
- [x] Identify new C#/.NET companies for cold outreach only when current technical evidence exists

## Review

- The strict pass found two usable company application routes with explicit C#/.NET requirements and three undated or older C#/.NET postings suitable only for a status-check email.
- Removed current but unsuitable results that required three or more years of experience, treated C# as one of several unrelated options, or described mainly frontend/full-stack work.
- Removed closed LinkedIn roles even when search snippets made them appear recent, and treated Worklab and its Zemedya brand as one employer rather than two opportunities.
- No newly verified C#/.NET opening close to Beylikduzu survived the filter; the strongest new remote route is Worklab and the strongest dated vacancy is Ainos in Cekmekoy.

# 2026-07-30 BankApp backend re-audit

- [x] Inventory the current backend project, startup configuration, dependencies, and registered services
- [x] Trace authentication, JWT issuance, role policies, identity extraction, and password/reset flows
- [x] Trace every controller through its service and data-access implementation
- [x] Match DTO and command parameters against SQL procedures, tables, and result mappings
- [x] Inspect financial transaction boundaries, approval workflows, background jobs, and concurrency controls
- [x] Inspect exception translation, validation, logging, SignalR, scheduled processing, and tests
- [x] Record verified architecture, feature flows, current risks, and runtime checks left for the user

## Review

- Source reviewed: the ASP.NET Core project, 15 controllers and 85 HTTP endpoints, service/data-access interfaces and implementations, DTO mappings, authentication helpers, SignalR hub, chat integration, two hosted services, `migration.sql`, `seed.sql`, and `triggers.sql`.
- Database contract reviewed: 23 tables (including history tables), 70 stored procedures, 9 triggers, financial transaction boundaries, row locks, and result-column mappings.
- Main risks found: customer loan ownership is not enforced on detail/payment operations; customer `transfer-between` bypasses the hold flow and does not require target ownership; recent-transfer lookup accepts an unowned account ID; broad employee permissions expose administrative mutations and password hashes; the loan processor fallback SQL contains a syntax error; loan eligibility mixes currency balances; and several approval/audit fields are ignored or absent.
- Verification boundary: this was a static source/SQL audit. Codex did not start or build the API, connect to SQL Server, call Groq/Frankfurter/SMTP, or run runtime tests. No test project exists in the repository.

# Proposed EFT simulation

- [x] Define transfer states, approval rules, API contracts, and failure behavior before coding
  - [x] Add the BankApp EFT entity, customer creation contract, detail contract, and shared status constants
  - [x] Review the first model/contracts together before adding persistence
- [x] Add BankApp EFT persistence, balance reservation, customer endpoints, and staff approval endpoints
  - [x] Add EFT, status-history, and outbox tables to the baseline migration
  - [x] Add atomic customer creation, list, and ownership-scoped detail procedures
  - [x] Add the customer EFT controller, service, data-access layer, and dependency registrations
  - [x] Add staff pending-list, approval, and rejection operations
- [x] Create an independent central payment-switch API and database
  - [x] Define the BankApp-to-switch request and response contracts
  - [x] Create the independent TCMB SQL Server database schema and participant seed
  - [x] Create the TCMB simulator project, authentication, persistence, and acceptance endpoint
- [ ] Create an independent recipient-bank API and database
- [ ] Add transactional outbox/inbox processing and idempotency at every network boundary
  - [x] Add BankApp outbox read, success, and failure procedures
  - [x] Add the signed BankApp-to-TCMB HTTP client
  - [x] Add and register the BankApp EFT outbox worker
  - [x] Source-review the sender and receiver signing contracts
- [ ] Add authenticated callbacks or status polling between the three systems
- [ ] Add customer status history, refund handling, audit records, and SignalR notifications
- [ ] Verify successful, rejected, duplicated, timed-out, and retried transfer scenarios

## Proposed boundaries

- The browser calls only BankApp. BankApp sends server-to-server payment instructions to the payment switch.
- The payment switch stores routing and settlement state, not customer account records.
- The recipient bank owns and updates its own customer/account database through its API.
- Each service commits only its own database transaction; cross-service consistency uses transfer states, idempotency, and outbox/inbox messages.

## Current EFT review

- `migration.sql` keeps the existing organization: EFT drop statements are in the drop section, EFT tables are beside the other tables, and all six EFT procedures are grouped under the dedicated `SPs — EFT` section.
- BankApp now has local EFT creation, customer list/detail access, idempotency, status history, holding-account reservation, and durable queued outbox records.
- Amounts above 5,000 TRY stop at `PendingApproval`; amounts at or below 5,000 TRY become `Queued` and receive a `SubmitEft` outbox message.
- The SQL procedure performs the EFT insert, source/holding balance movements, transaction records, status history, and outbox insert in one transaction.
- Reusing a request ID with identical data succeeds without reserving money again; reusing it with changed data is rejected.
- The customer EFT POST currently returns a message-only `Result`; customers retrieve created records through the list or detail endpoints.
- Staff can now list `PendingApproval` EFTs, approve them into `Queued`, or reject them with a required reason.
- Approval records the employee and creates the durable `SubmitEft` message without moving the already reserved funds.
- Rejection returns the reserved amount from the holding account, records both account transactions, and stores the employee and reason in status history.
- No outbox worker, TCMB request, receiver-bank credit, callback, external-failure refund, or frontend has been added yet.
- The TCMB simulator now accepts authenticated `POST /api/payments` requests, derives the receiver bank from the IBAN, and stores `Accepted` payment orders idempotently.
- TCMB requests use `X-Bank-Code`, `X-Timestamp`, and an HMAC-SHA256 `X-Signature`; shared bank secrets stay outside source-controlled configuration.
- TCMB now exposes Swagger UI in development and documents all three bank-authentication headers through its Authorize dialog.
- BankApp now polls queued `SubmitEft` outbox messages, signs the exact serialized JSON body, and submits it to TCMB with the three authentication headers.
- A TCMB acceptance updates the BankApp EFT to `Submitted` and completes the outbox message in one local transaction; failed calls remain retryable and move to `PendingReconciliation` after the configured attempt limit.
- TCMB acceptance does not route or credit the recipient bank yet. Application build, SQL execution, and runtime request verification remain with the user.
