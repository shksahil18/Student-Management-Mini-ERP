# Testing Strategy & Quality Assurance Plan

## 1. Unit & Integration Testing Strategy
- **Backend API Layer:** Verify controllers, status codes (200, 201, 400, 401, 403, 404, 409).
- **Service & Business Rules:**
  - Fee payment atomic updates.
  - Overpayment rejection test (assert exception/error when `Payment > Outstanding`).
  - Unique student code generation collision handling.
  - BCrypt password verification.
- **Database Layer:** EF Core migrations and SQL constraint checks (check constraint `PaidAmount <= TotalAmount`).

## 2. Manual & Exploratory Verification Plan
1. **Authentication Flow:**
   - Valid admin login -> Token stored in browser session -> Protected routes accessible.
   - Invalid password -> 401 Unauthorized with clear message.
   - Token expiration / manual logout -> Redirect to `/login`.
2. **Student Flow:**
   - Admission with all fields -> Success notification, redirects to list/details.
   - Duplicate email / code -> 400 Validation error properly displayed under field.
   - Soft deactivation -> Toggle active status, inspect indicator and dashboard count.
   - Server-side Search & Pagination -> Search for partial name, verify count and page items.
3. **Fee & Payment Flow:**
   - Create fee of ₹50,000.
   - Submit partial payment of ₹20,000 -> Verify outstanding drops to ₹30,000, receipt issued.
   - Attempt payment of ₹35,000 -> Verify rejection with message "Amount exceeds remaining outstanding balance of ₹30,000".
   - Submit payment of ₹30,000 -> Outstanding drops to 0, status changes to "Paid".
4. **Dashboard Aggregation:**
   - Verify all KPIs (Total Students, Active/Inactive, Billed, Collected, Outstanding) match database values exactly.
