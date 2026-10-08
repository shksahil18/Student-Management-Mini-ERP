# Additional Change Request (CR-01): Printable Payment Receipt & CSV Data Export

## 1. Overview & Business Justification
In educational ERP operations, parents and administrative staff frequently require:
1. **Official Printable Payment Receipts** (including institution header, student details, breakdown of amount paid, remaining balance, payment method, date, and cashier signature line).
2. **Student & Fee Data Export to CSV** for offline accounting and reporting.

## 2. Technical Impact Analysis
- **Database:** No schema breaking changes required; existing `Payment`, `Fee`, and `Student` relationships supply all needed data.
- **Backend API:**
  - Add `GET /api/payments/{receiptNumber}/print` endpoint returning structured receipt metadata formatted for templating.
  - Add `GET /api/students/export` endpoint producing a downloadable CSV stream.
- **Frontend UI:**
  - Add a dedicated "Print Receipt" component formatted with CSS `@media print` rules for clean single-page printing / Save-as-PDF.
  - Add an "Export CSV" button to the Student Listing page that triggers instant download.

## 3. Implementation Steps
1. Add receipt print preview modal with CSS print styling.
2. Add CSV export endpoint and client hook.
3. Test receipt print dialog across various payment amounts (full, partial).
4. Update API docs and testing guide.
