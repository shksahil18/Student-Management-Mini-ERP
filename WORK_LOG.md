# Student Management Mini-ERP — Work Log & Engineering Decisions

## Milestone Log

### Day 1: Architecture, Blueprint & Database Foundation
- **Architecture Design:** Designed 3-tier REST architecture using ASP.NET Core 9 Web API, Entity Framework Core 9, Microsoft SQL Server, and React 18 (Vite 6).
- **Relational Integrity:** Defined entities `User`, `Student`, `Fee`, and `Payment` with strict foreign keys (`DeleteBehavior.Restrict`), check constraints, unique indexes, and `decimal(18,2)` currency precision.
- **Transaction Safety Strategy:** Ensured financial transactions use `IDbContextTransaction` for atomic updates to `Fee` balances and `Payment` receipts with balance checking.
- **Documentation:** Created complete specifications: `architecture.md`, `database.md`, `api.md`, `testing.md`, `change-request.md`, and task tracking plans.
- **Database Scripts:** Created `database/create-database.sql` and `database/seed-data.sql`, deployed and verified against Microsoft SQL Server (`StudentManagementDb`).

### Day 2: Backend API Implementation & Verification
- **ASP.NET Core 9 Web API:** Scaffolded `StudentManagement.Api` with EF Core SQL Server, JWT Bearer authentication, and Swashbuckle OpenAPI.
- **DTOs & Controllers:** Implemented clean DTO contracts for Auth, Students (with `StudentQueryParameters`), Fees, and Dashboard analytics.
- **Authentication & Security:** Implemented BCrypt password hashing and JWT token issuance. Updated `DbInitializer.cs` to ensure BCrypt hashes are dynamically synchronized on startup to resolve stale SQL seed hashes.
- **Business Logic Services:**
  - `StudentService`: Server-side search, filtering by Course/Status, pagination (`PagedResultDto`), soft deactivation, and student code auto-generation (`STU-YYYY-XXXXX`).
  - `FeeService`: ACID-compliant payment processing, overpayment validation guard, remaining balance recalculation, and unique receipt generation (`REC-YYYY-XXXXXX`).
  - `DashboardService`: Aggregated SQL metrics (student counts, fees billed, collected, outstanding, recent feeds).
- **Global Error Handling:** Implemented `ExceptionHandlingMiddleware` for consistent JSON error envelopes (`ApiResponse<T>`).
- **API Verification:** Validated all endpoints via Swagger & CLI calls (Login, Dashboard, Student listing/search, partial payments, and 400 rejection on overpayment).

### Day 3: React Frontend Development & Change Request CR-01
- **Vite + React Setup:** Configured frontend with Vite 6, React 18, React Router v6, Axios, and Lucide React icons.
- **Auth & Layout:** Built `AuthContext` for JWT lifecycle management, Axios interceptor for Bearer tokens & 401 redirection, responsive Sidebar, Navbar, and role-based `ProtectedRoute`.
- **Core Modules:**
  - **Login:** Interactive credentials selector with one-click demo pills.
  - **Dashboard:** 6 real-time KPI metric cards, recent payments timeline, recent admissions table.
  - **Student Management:** Admission form with validation, paginated student directory with search/filters, full profile view, student edit form, and soft deactivation toggle.
  - **Fee Management:** All-fees ledger, filter by status, and fee assignment.
  - **Payment Processing:** Modal with balance display, "Pay Full" shortcut, payment method selector, and live backend submission.
- **Change Request (CR-01):**
  - **Printable Receipts:** Implemented `ReceiptModal` with `@media print` styling, receipt breakdown, and one-click browser print dialog.
  - **CSV Export:** Implemented export endpoints and frontend one-click export for student lists and financial summaries.
- **Testing & Build Verification:** Completed `npm run build` (0 errors), tested end-to-end integration with the backend API.
