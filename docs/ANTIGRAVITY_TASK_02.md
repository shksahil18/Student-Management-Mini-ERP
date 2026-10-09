# Task 02: Full-Stack Integration & Verification Spec

## Objective
Verify end-to-end integration between React frontend, ASP.NET Core 9 Web API, and Microsoft SQL Server database.

## Test Verification Checklist
- [x] **Authentication Flow**:
  - `admin` login with `Admin@123` -> returns valid JWT Bearer token and role claims.
  - `staff` login with `Staff@123` -> restricted from admission and edit routes.
  - Invalid credentials -> returns HTTP 401 with standard error envelope.
- [x] **Dashboard Metrics**:
  - Displays total students (6), active (5), inactive (1).
  - Financial summary: Total Billed ₹2,95,000, Total Collected ₹130,000, Total Outstanding ₹165,000.
  - Recent payments timeline and recent admissions list rendered dynamically.
- [x] **Student Management**:
  - Paginated list queries `/api/students?page=1&pageSize=5`.
  - Server-side search by name/code and filtering by course/status.
  - Admission form creates student with generated code `STU-YYYY-XXXXX`.
  - Soft deactivation toggles `IsActive` without deleting database records.
- [x] **Fee Management & ACID Payments**:
  - View fee records and balances.
  - Partial payment updates outstanding balance atomically.
  - Overpayment exceeding balance is rejected with HTTP 400.
  - Unique receipt code `REC-YYYY-XXXXXX` generated and viewable in printable modal.
- [x] **Change Request CR-01**:
  - Printable receipt modal triggers `@media print` cleanly.
  - CSV export downloads valid formatted CSV file.
